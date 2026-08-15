using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace TrustPay.Application.Common.Transactions.Commands.ProcessBankWebhook
{
    public class ProcessBankWebhookCommandValidator : AbstractValidator<ProcessBankWebhookCommand>
    {
        public ProcessBankWebhookCommandValidator()
        {
            RuleFor(x=>x.TransactionId )
                .NotEqual(Guid.Empty)
                .WithMessage("ID транзакции не может быть пустым");
            When(x => x.IsSuccess, () =>
            {
                RuleFor(x => x.ExternalPaymentId)
                .NotEmpty()
                .WithMessage("При успешной оплате ExternalPaymentId обязателен.")
                .MaximumLength(100)
                .WithMessage("ExternalPaymentId не может превышать 100 символов.");

            })
                .Otherwise(() =>
                    {
                        RuleFor(x => x.FailureReason)
                            .NotEmpty()
                            .WithMessage("При неудачной оплате должна быть указана причина.")
                            .MaximumLength(500)
                            .WithMessage("Причина ошибки не должна превышать 500 символов.");
                    });

            

                
                

        }
    }
}
