using FluentValidation;

namespace TrustPay.Application.Wallets.Queries.GetWalletByUserId
{
    public class GetWalletByUserIdQueryValidator : AbstractValidator<GetWalletByUserIdQuery>
    {
        public GetWalletByUserIdQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Идентификатор пользователя не может быть пустым.");
        }
    }
}