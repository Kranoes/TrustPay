using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Transactions.Queries.GetTransactionById;

public record GetTransactionByIdQuery(Guid TransactionId) : IRequest<Result<TransactionResponse>>; 
    public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, Result<TransactionResponse>>
{
private readonly ITrustPayDbContext _context;

    public GetTransactionByIdQueryHandler(ITrustPayDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TransactionResponse>> Handle(
        GetTransactionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
        {
            return Result.Failure<TransactionResponse>(
                Error.NotFound("Transaction.NotFound", $"Транзакция с ID '{request.TransactionId}' не найдена."));
        }

        var response = new TransactionResponse(
            transaction.Id,
            transaction.SenderWalletId,
            transaction.ReceiverWalletId,
            transaction.Amount.Amount,
            transaction.Amount.Currency,
            transaction.Type.ToString(),
            transaction.Status.ToString(),
            transaction.CreatedAt);

        return Result.Success(response);
    }
    }