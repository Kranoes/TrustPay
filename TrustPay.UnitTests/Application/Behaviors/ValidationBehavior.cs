using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using TrustPay.Application.Common.Behaviors;

namespace TrustPay.UnitTests.Application.Common.Behaviors
{
    public class ValidationBehaviorTests
    {
        public record TestRequest(string Data) : MediatR.IRequest<string>;

        [Fact]
        public async Task Handle_ShouldCallNext_WhenNoValidatorsExist()
        {
            var request = new TestRequest("TestData");
            var expectedResponse = "Success";
            var behavior = new ValidationBehavior<TestRequest, string>(Enumerable.Empty<IValidator<TestRequest>>());

            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();
            nextDelegate.Invoke().Returns(Task.FromResult(expectedResponse));

            var response = await behavior.Handle(request, nextDelegate, CancellationToken.None);

            response.Should().Be(expectedResponse);
            await nextDelegate.Received(1).Invoke();
        }

        [Fact]
        public async Task Handle_ShouldCallNext_WhenValidationSucceeds()
        {
            var request = new TestRequest("ValidData");
            var expectedResponse = "Success";

            var validatorMock = Substitute.For<IValidator<TestRequest>>();
            validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ValidationResult()));

            var behavior = new ValidationBehavior<TestRequest, string>(new[] { validatorMock });

            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();
            nextDelegate.Invoke().Returns(Task.FromResult(expectedResponse));

            var response = await behavior.Handle(request, nextDelegate, CancellationToken.None);

            response.Should().Be(expectedResponse);
            await nextDelegate.Received(1).Invoke();
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationExceptionAndNotCallNext_WhenValidationFails()
        {
            var request = new TestRequest("InvalidData");
            var failures = new List<ValidationFailure> { new("Data", "Field cannot be empty") };

            var validatorMock = Substitute.For<IValidator<TestRequest>>();
            validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ValidationResult(failures)));

            var behavior = new ValidationBehavior<TestRequest, string>(new[] { validatorMock });

            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();

            var act = () => behavior.Handle(request, nextDelegate, CancellationToken.None);

            var exception = await act.Should().ThrowAsync<ValidationException>();
            exception.Which.Errors.Should().ContainSingle(e => e.ErrorMessage == "Field cannot be empty");

            await nextDelegate.DidNotReceive().Invoke();
        }

        [Fact]
        public async Task Handle_ShouldAggregateFailuresFromMultipleValidators_WhenMultipleValidatorsFail()
        {
            var request = new TestRequest("InvalidData");

            var validatorMock1 = Substitute.For<IValidator<TestRequest>>();
            validatorMock1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ValidationResult(new[] { new ValidationFailure("Data", "Error 1") })));

            var validatorMock2 = Substitute.For<IValidator<TestRequest>>();
            validatorMock2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ValidationResult(new[] { new ValidationFailure("Data", "Error 2") })));

            var behavior = new ValidationBehavior<TestRequest, string>(new[] { validatorMock1, validatorMock2 });

            var nextDelegate = Substitute.For<MediatR.RequestHandlerDelegate<string>>();

            var act = () => behavior.Handle(request, nextDelegate, CancellationToken.None);

            var exception = await act.Should().ThrowAsync<ValidationException>();
            exception.Which.Errors.Should().HaveCount(2);
            exception.Which.Errors.Select(e => e.ErrorMessage).Should().Contain(new[] { "Error 1", "Error 2" });

            await nextDelegate.DidNotReceive().Invoke();
        }
    }
}