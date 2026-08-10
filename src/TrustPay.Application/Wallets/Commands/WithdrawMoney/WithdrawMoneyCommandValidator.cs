using FluentValidation;
using TrustPay.Application.Wallets.Commands.WithdrawMoney;

public class WithdrawMoneyCommandValidator : AbstractValidator<WithdrawMoneyCommand>
{
    public WithdrawMoneyCommandValidator()
    {
        RuleFor(x => x.WalletId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Сумма списания должна быть больше 0.");
        RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("Код валюты должен состоять из 3 символов.");
    }
}