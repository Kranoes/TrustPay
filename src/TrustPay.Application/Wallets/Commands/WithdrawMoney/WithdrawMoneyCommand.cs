namespace TrustPay.Application.Wallets.Commands.WithdrawMoney;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;

public record WithdrawMoneyCommand(
    Guid WalletId,
    decimal Amount,
    string Currency = "RUB") : IRequest<Result>;

public class WithdrawMoneyCommandHandler : IRequestHandler<WithdrawMoneyCommand, Result>
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public WithdrawMoneyCommandHandler(
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(WithdrawMoneyCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Error.NotFound("Wallet.NotFound", $"Кошелек с ID '{request.WalletId}' не найден.");
        }

        if (wallet.UserId != _currentUserService.UserId && !_currentUserService.IsAdmin)
        {
            return Error.Forbidden("Wallet.Forbidden", "У вас нет прав на списание средств с данного кошелька.");
        }

        var moneyResult = Money.Create(request.Amount, request.Currency);
        if (moneyResult.IsFailure)
        {
            return Result.Failure(moneyResult.Error);
        }

        var withdrawResult = wallet.Withdraw(moneyResult.Value);
        if (withdrawResult.IsFailure)
        {
            return withdrawResult;
        }

        var transactionResult = Transaction.CreateWithdrawal(wallet.Id, moneyResult.Value);
        if (transactionResult.IsFailure)
        {
            return Result.Failure(transactionResult.Error);
        }

        await _transactionRepository.AddAsync(transactionResult.Value, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict("Wallet.ConcurrencyConflict", "Баланс кошелька был изменен другим запросом. Повторите попытку.");
        }
    }
}