using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TrustPay.Application.Common.Behaviors;

namespace TrustPay.UnitTests.Application.Common.Behaviors
{
    public class LoggingBehaviorTests
    {
        public record TestRequest(string Data) : MediatR.IRequest<string>;

        private readonly ILogger<LoggingBehavior<TestRequest, string>> _loggerMock;
        private readonly LoggingBehavior<TestRequest, string> _behavior;

        public LoggingBehaviorTests()
        {
            _loggerMock = Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();
            _behavior = new LoggingBehavior<TestRequest, string>(_loggerMock);
        }

        [Fact]
        public async Task Handle_ShouldCallNextAndLogInformation_WhenExecutionIsFast()
        {
            var request = new TestRequest("TestData");
            var expectedResponse = "Success";
            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();
            nextDelegate.Invoke().Returns(Task.FromResult(expectedResponse));

            var response = await _behavior.Handle(request, nextDelegate, CancellationToken.None);

            response.Should().Be(expectedResponse);
            await nextDelegate.Received(1).Invoke();

            _loggerMock.Received(2).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task Handle_ShouldLogWarning_WhenExecutionExceedsThreshold()
        {
            var request = new TestRequest("SlowData");
            var expectedResponse = "Success";
            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();

            nextDelegate.Invoke().Returns(async _ =>
            {
                await Task.Delay(510);
                return expectedResponse;
            });

            var response = await _behavior.Handle(request, nextDelegate, CancellationToken.None);

            response.Should().Be(expectedResponse);

            _loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());

            _loggerMock.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}