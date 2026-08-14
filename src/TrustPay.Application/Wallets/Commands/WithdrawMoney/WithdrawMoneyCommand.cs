namespace TrustPay.Application.Wallets.Commands.WithdrawMoney;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

public record WithdrawMoneyCommand(
    Guid WalletId,
    decimal Amount,
    string Currency = "RUB") : IRequest<Result>;

public class WithdrawMoneyCommandHandler : IRequestHandler<WithdrawMoneyCommand, Result>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WithdrawMoneyCommandHandler(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(WithdrawMoneyCommand request, CancellationToken cancellationToken)
    {
        var moneyResult = Money.Create(request.Amount, request.Currency);
        if (moneyResult.IsFailure)
        {
            return Result.Failure(moneyResult.Error);
        }

        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Error.NotFound("Wallet.NotFound", $"Кошелек с ID '{request.WalletId}' не найден");
        }

        Result withdrawResult = wallet.Withdraw(moneyResult.Value);
        if (withdrawResult.IsFailure)
        {
            return withdrawResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}