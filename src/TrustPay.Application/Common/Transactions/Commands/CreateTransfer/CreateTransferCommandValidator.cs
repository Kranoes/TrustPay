using FluentValidation;

namespace TrustPay.Application.Common.Transactions.Commands.CreateTransfer
{
    public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
    {
        public CreateTransferCommandValidator()
        {
            RuleFor(x => x.SenderWalletId)
                .NotEmpty()
                .WithMessage("Идентификатор кошелька отправителя не может быть пустым.");

            RuleFor(x => x.ReceiverWalletId)
                .NotEmpty()
                .WithMessage("Идентификатор кошелька получателя не может быть пустым.")
                .NotEqual(x => x.SenderWalletId)
                .WithMessage("Кошелек получателя не может совпадать с кошельком отправителя.");

            RuleFor(x => x.Amount)
                .NotNull()
                .WithMessage("Сумма должна быть указана.");

            When(x => x.Amount != null, () =>
            {
                RuleFor(x => x.Amount.Amount)
                    .GreaterThan(0)
                    .WithMessage("Сумма перевода должна быть больше нуля.");

                RuleFor(x => x.Amount.Currency)
                    .NotEmpty()
                    .WithMessage("Валюта должна быть указана.");
            });
        }
    }
}