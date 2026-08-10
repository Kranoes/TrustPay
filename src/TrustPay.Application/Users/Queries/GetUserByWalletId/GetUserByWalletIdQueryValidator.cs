namespace TrustPay.Application.Users.Queries.GetUserByWalletId;

using FluentValidation;

public class GetUserByWalletIdQueryValidator : AbstractValidator<GetUserByWalletIdQuery>
{
    public GetUserByWalletIdQueryValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("Идентификатор кошелька не может быть пустым.");
    }
}