namespace TrustPay.Application.Wallets.Commands.UnfreezeWallet;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record UnfreezeWalletCommand(Guid WalletId) : IRequest<Result>;

public class UnfreezeWalletCommandHandler : IRequestHandler<UnfreezeWalletCommand, Result>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnfreezeWalletCommandHandler(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UnfreezeWalletCommand command, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByIdAsync(command.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Error.NotFound("Wallet.NotFound", $"Кошелек с ID '{command.WalletId}' не найден");
        }

        Result unfreezeResult = wallet.Unfreeze();
        if (unfreezeResult.IsFailure)
        {
            return unfreezeResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}