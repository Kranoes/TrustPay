using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TrustPay.Application.Common.Behaviors;

namespace TrustPay.UnitTests.Application.Common.Behaviors
{
    public class PerformanceBehaviorTests
    {
        public record TestRequest(string Data) : MediatR.IRequest<string>;

        private readonly ILogger<PerformanceBehavior<TestRequest, string>> _loggerMock;
        private readonly PerformanceBehavior<TestRequest, string> _behavior;

        public PerformanceBehaviorTests()
        {
            _loggerMock = Substitute.For<ILogger<PerformanceBehavior<TestRequest, string>>>();
            _behavior = new PerformanceBehavior<TestRequest, string>(_loggerMock);
        }

        [Fact]
        public async Task Handle_ShouldNotLogWarning_WhenRequestIsFast()
        {
            var request = new TestRequest("FastData");
            var expectedResponse = "Success";
            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();
            nextDelegate.Invoke().Returns(Task.FromResult(expectedResponse));

            var response = await _behavior.Handle(request, nextDelegate, CancellationToken.None);

            response.Should().Be(expectedResponse);
            await nextDelegate.Received(1).Invoke();

            _loggerMock.DidNotReceiveWithAnyArgs().Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task Handle_ShouldLogWarning_WhenRequestExceedsThreshold()
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
            await nextDelegate.Received(1).Invoke();

            _loggerMock.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}