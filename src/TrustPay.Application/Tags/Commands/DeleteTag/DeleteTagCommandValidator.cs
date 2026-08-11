namespace TrustPay.Application.Tags.Commands.DeleteTag;

using FluentValidation;

public class DeleteTagCommandValidator : AbstractValidator<DeleteTagCommand>
{
    public DeleteTagCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Идентификатор тега обязателен.");
    }
}