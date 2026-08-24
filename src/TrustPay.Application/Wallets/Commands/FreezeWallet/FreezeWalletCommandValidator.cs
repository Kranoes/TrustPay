namespace TrustPay.Application.Wallets.Commands.FreezeWallet;

using FluentValidation;

public class FreezeWalletCommandValidator : AbstractValidator<FreezeWalletCommand>
{
    public FreezeWalletCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty()
            .WithMessage("Идентификатор кошелька не может быть пустым.");
    }
}