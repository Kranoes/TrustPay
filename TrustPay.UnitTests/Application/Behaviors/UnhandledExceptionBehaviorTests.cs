using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TrustPay.Application.Common.Behaviors;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Behaviors
{
    public class UnhandledExceptionBehaviorTests
    {
        public record TestRequest(string Data) : MediatR.IRequest<string>;

        private readonly ILogger<UnhandledExceptionBehavior<TestRequest, string>> _loggerMock;
        private readonly UnhandledExceptionBehavior<TestRequest, string> _behavior;

        public UnhandledExceptionBehaviorTests()
        {
            _loggerMock = Substitute.For<ILogger<UnhandledExceptionBehavior<TestRequest, string>>>();
            _behavior = new UnhandledExceptionBehavior<TestRequest, string>(_loggerMock);
        }

        [Fact]
        public async Task Handle_ShouldReturnResponse_WhenNoExceptionThrown()
        {
            var request = new TestRequest("TestData");
            var expectedResponse = "Success";
            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();
            nextDelegate.Invoke().Returns(Task.FromResult(expectedResponse));

            var response = await _behavior.Handle(request, nextDelegate, CancellationToken.None);

            response.Should().Be(expectedResponse);
            await nextDelegate.Received(1).Invoke();
        }

        [Fact]
        public async Task Handle_ShouldLogAndRethrow_WhenUnhandledExceptionOccurs()
        {
            var request = new TestRequest("TestData");
            var exception = new InvalidOperationException("Database connection failed");
            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();
            nextDelegate.Invoke().ThrowsAsync(exception);

            var act = () => _behavior.Handle(request, nextDelegate, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _loggerMock.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task Handle_ShouldRethrowWithoutLoggingError_WhenValidationExceptionOccurs()
        {
            var request = new TestRequest("TestData");
            var validationException = new ValidationException(new[] { new ValidationFailure("Field", "Required") });
            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();
            nextDelegate.Invoke().ThrowsAsync(validationException);

            var act = () => _behavior.Handle(request, nextDelegate, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();

            _loggerMock.DidNotReceiveWithAnyArgs().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}