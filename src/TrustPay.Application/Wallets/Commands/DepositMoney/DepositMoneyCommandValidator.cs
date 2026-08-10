using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace TrustPay.Application.Wallets.Commands.DepositMoney
{
    public class DepositMoneyCommandValidator : AbstractValidator<DepositMoneyCommand>
    {
        public DepositMoneyCommandValidator()
        {
            RuleFor(x => x.WalletId)
                .NotEmpty().WithMessage("ID кошелька обязателен.");
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Сумма пополнения должна быть больше нуля");
            RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("Код валюты должен состоять из 3 символов.");
        }
    }
}
