using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Transactions.Queries.GetTransactionById;

public record GetTransactionByIdQuery(Guid TransactionId) : IRequest<Result<TransactionResponse>>; 
    public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, Result<TransactionResponse>>
{
private readonly ITrustPayDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionByIdQueryHandler(ITrustPayDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
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

        if (!_currentUserService.IsAdmin)
        {
            var currentUserId = _currentUserService.UserId;
            var ownsWallet = await _context.Wallets
                .AsNoTracking()
                .AnyAsync(w => w.UserId == currentUserId
                    && (w.Id == transaction.SenderWalletId || w.Id == transaction.ReceiverWalletId),
                    cancellationToken);

            if (!ownsWallet)
            {
                return Result.Failure<TransactionResponse>(
                    Error.Forbidden("Transaction.Forbidden", "У вас нет прав на просмотр данной транзакции."));
            }
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