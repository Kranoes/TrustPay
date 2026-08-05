using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using MediatR;
using TrustPay.Application.Common.Models;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Common.Transactions.Queries.GetWalletTransactions
{
    public record GetWalletTransactionsQuery(Guid WalletId, int PageNumber = 1, int PageSize = 10) :  IRequest<Result<PageResult<TransactionResponse>>>;
    public class GetWalletTransactionsQueryValidator : AbstractValidator<GetWalletTransactionsQuery>
    {
        public GetWalletTransactionsQueryValidator()
        {
            RuleFor(x => x.WalletId)
                .NotEmpty()
                .WithMessage("Идентификатор кошелька обязателен.");

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Номер страницы должен быть не меньше 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Размер страницы должен быть от 1 до 100 записей.");
        }
    }
}
