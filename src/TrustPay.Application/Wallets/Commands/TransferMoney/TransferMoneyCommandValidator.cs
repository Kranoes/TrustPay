using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace TrustPay.Application.Wallets.Commands.TransferMoney
{
    public class TransferMoneyCommandValidator : AbstractValidator<TransferMoneyCommand>
    {
        public TransferMoneyCommandValidator() 
        {
            RuleFor(x => x.Amount)  
                   .GreaterThan(0)
                   .WithMessage("Сумма перевода не может быть отрицательной или равняться нулю");
            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .WithMessage("Валюта должна состоять ровно из 3 символов");
            RuleFor(x => x.WalletIdSender)
                .NotEmpty()
                .WithMessage("Не указан ID отправителя")
                .NotEqual(x => x.WalletIdReceive)
                .WithMessage("Нельзя перевести деньги на кошелек с которого они были отправлены");
            RuleFor(x => x.WalletIdReceive)
                .NotEmpty()
                .WithMessage("Не указан ID получателя");
        }
    }
}
