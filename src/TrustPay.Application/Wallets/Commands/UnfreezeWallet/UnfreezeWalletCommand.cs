namespace TrustPay.Application.Wallets.Commands.UnfreezeWallet;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record UnfreezeWalletCommand(Guid WalletId) : IRequest<Result>;

public class UnfreezeWalletCommandHandler : IRequestHandler<UnfreezeWalletCommand, Result>
{
    private readonly IWalletRepository _walletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UnfreezeWalletCommandHandler(
        IWalletRepository walletRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UnfreezeWalletCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin && !_currentUserService.IsArbitrator)
        {
            return Error.Forbidden("Wallet.Forbidden", "Недостаточно прав для разморозки кошелька.");
        }

        var wallet = await _walletRepository.GetByIdAsync(command.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Error.NotFound("Wallet.NotFound", $"Кошелек с ID '{command.WalletId}' не найден.");
        }

        var unfreezeResult = wallet.Unfreeze();
        if (unfreezeResult.IsFailure)
        {
            return unfreezeResult;
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict("Wallet.ConcurrencyConflict", "Состояние кошелька было изменено другим запросом. Повторите попытку.");
        }
    }
}