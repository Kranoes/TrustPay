using FluentValidation;

namespace TrustPay.Application.Common.Transactions.Commands.CreateWithdrawal
{
    public class CreateWithdrawalCommandValidator : AbstractValidator<CreateWithdrawalCommand>
    {
        public CreateWithdrawalCommandValidator()
        {
            RuleFor(x => x.SenderWalletId)
                .NotEmpty()
                .WithMessage("Идентификатор кошелька отправителя не может быть пустым.");

            RuleFor(x => x.Amount)
                .NotNull()
                .WithMessage("Сумма должна быть указана.");

            When(x => x.Amount != null, () =>
            {
                RuleFor(x => x.Amount.Amount)
                    .GreaterThan(0)
                    .WithMessage("Сумма вывода должна быть больше нуля.");

                RuleFor(x => x.Amount.Currency)
                    .NotEmpty()
                    .WithMessage("Валюта должна быть указана.");
            });
        }
    }
}