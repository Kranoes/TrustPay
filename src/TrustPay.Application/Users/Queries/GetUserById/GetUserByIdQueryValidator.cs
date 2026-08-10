namespace TrustPay.Application.Users.Queries.GetUserById;

using FluentValidation;

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Идентификатор пользователя не может быть пустым.");
    }
}