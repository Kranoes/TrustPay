using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace TrustPay.Application.Wallets.Commands.UnfreezeWallet
{
    public class UnfreezeWalletCommandValidator : AbstractValidator<UnfreezeWalletCommand>
    {
        public UnfreezeWalletCommandValidator()
        {
            RuleFor(x => x.WalletId).NotEmpty().WithMessage("Id кошелька обязателен.");
        }
    }
}
