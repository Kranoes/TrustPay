namespace TrustPay.Application.Wallets.Commands.FreezeWallet;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record FreezeWalletCommand(Guid WalletId) : IRequest<Result>;

public class FreezeWalletCommandHandler : IRequestHandler<FreezeWalletCommand, Result>
{
    private readonly IWalletRepository _walletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public FreezeWalletCommandHandler(
        IWalletRepository walletRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(FreezeWalletCommand command, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAdmin && !_currentUserService.IsArbitrator)
        {
            return Error.Forbidden("Wallet.Forbidden", "Недостаточно прав для заморозки кошелька.");
        }

        var wallet = await _walletRepository.GetByIdAsync(command.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Error.NotFound("Wallet.NotFound", $"Кошелек с ID '{command.WalletId}' не найден.");
        }

        var freezeResult = wallet.Freeze();
        if (freezeResult.IsFailure)
        {
            return freezeResult;
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