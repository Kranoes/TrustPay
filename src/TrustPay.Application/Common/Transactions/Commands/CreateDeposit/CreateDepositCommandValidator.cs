using FluentValidation;

namespace TrustPay.Application.Common.Transactions.Commands.CreateDeposit
{
    public class CreateDepositCommandValidator : AbstractValidator<CreateDepositCommand>
    {
        public CreateDepositCommandValidator()
        {
            RuleFor(x => x.ReceiverWalletId)
                .NotEmpty()
                .WithMessage("Идентификатор кошелька не может быть пустым.");

            RuleFor(x => x.Amount)
                .NotNull()
                .WithMessage("Сумма должна быть указана.");

            When(x => x.Amount != null, () =>
            {
                RuleFor(x => x.Amount.Amount)
                    .GreaterThan(0)
                    .WithMessage("Сумма пополнения должна быть больше нуля.");

                RuleFor(x => x.Amount.Currency)
                    .NotEmpty()
                    .WithMessage("Валюта должна быть указана.");
            });
        }
    }
}