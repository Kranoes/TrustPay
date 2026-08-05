using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
namespace TrustPay.Application.Wallets.Commands.CreateWallet
{
    public class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
    {
        public CreateWalletCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("Идентификатор пользователя не может быть пустым.");
            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .WithMessage("Код валюты должен состоять ровно из 3 символов.");
            RuleFor(x => x.InitialAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Стартовый баланс не может быть отрицательным.");
        }
    }
}
