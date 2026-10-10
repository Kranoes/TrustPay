using MediatR;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Models;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Application.Common.Transactions.Queries.GetWalletTransactions;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Common.Transactions.Queries.GetWalletTransactions;

public class GetWalletTransactionsQueryHandler
    : IRequestHandler<GetWalletTransactionsQuery, Result<PageResult<TransactionResponse>>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetWalletTransactionsQueryHandler(
        ITransactionRepository transactionRepository,
        IWalletRepository walletRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PageResult<TransactionResponse>>> Handle(
        GetWalletTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure<PageResult<TransactionResponse>>("Кошелек не найден.");
        }

        if (wallet.UserId != _currentUserService.UserId && !_currentUserService.IsAdmin)
        {
            return Result.Failure<PageResult<TransactionResponse>>(
                Error.Forbidden("Wallet.Forbidden", "У вас нет прав на просмотр транзакций данного кошелька."));
        }

        var (items, totalCount) = await _transactionRepository.GetPagedByWalletIdAsync(
            request.WalletId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var responseItems = items.Select(t => new TransactionResponse(
            t.Id,
            t.SenderWalletId,
            t.ReceiverWalletId,
            t.Amount.Amount,
            t.Amount.Currency,
            t.Type.ToString(),
            t.Status.ToString(),
            t.CreatedAt)).ToList();

        var pagedResult = new PageResult<TransactionResponse>(
            responseItems,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result.Success(pagedResult);
    }
}