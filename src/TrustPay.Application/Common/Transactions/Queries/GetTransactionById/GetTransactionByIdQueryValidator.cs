using System;
using FluentValidation;
using TrustPay.Application.Transactions.Queries.GetTransactionById;

namespace TrustPay.Application.Common.Transactions.Queries.GetTransactionById;

public class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdQueryValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID транзакции не может быть пустым.");
    }
}