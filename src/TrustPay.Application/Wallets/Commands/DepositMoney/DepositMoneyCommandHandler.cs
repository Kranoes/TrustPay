namespace TrustPay.Application.Wallets.Commands.DepositMoney;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

public record DepositMoneyCommand (
    Guid WalletId,
    decimal Amount,
    string Currency
    ) : IRequest<Result>;   
public class DepositMoneyCommandHandler : IRequestHandler<DepositMoneyCommand, Result>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositMoneyCommandHandler(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DepositMoneyCommand request, CancellationToken cancellationToken)
    {
        var moneyResult = Money.Create(request.Amount, request.Currency);
        if (moneyResult.IsFailure)
        {
            return Result.Failure(moneyResult.Error);
        }

        Money money = moneyResult.Value;

        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure($"Кошелек с ID '{request.WalletId}' не найден");
        }

        Result depositResult = wallet.Deposit(money);
        if (depositResult.IsFailure)
        {
            return depositResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}