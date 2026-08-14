using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Application.Wallets.Commands.CreateWallet
{
    public record CreateWalletCommand(
        Guid UserId,
        decimal InitialAmount = 0,
        string Currency = "RUB") : IRequest<Result<Guid>>;

    public class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, Result<Guid>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        public CreateWalletCommandHandler(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
        {
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Error.NotFound("User.NotFound", $"Пользователь с ID {request.UserId} не найден.");
            }
            var existingWallet = await _walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (existingWallet is not null)
            {
                return Error.Conflict("Wallet.AlreadyExists", "У этого пользователя уже создан кошелек.");
            }
            var moneyResult = Money.Create(request.InitialAmount, request.Currency);
            if (moneyResult.IsFailure)
            {
                return Result.Failure<Guid>(moneyResult.Error);
            }

            var walletResult = Wallet.Create(request.UserId, moneyResult.Value);
            if (walletResult.IsFailure)
            {
                return Result.Failure<Guid>(walletResult.Error);
            }

            var wallet = walletResult.Value;

            await _walletRepository.AddAsync(wallet, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(wallet.Id);
        }
    }
}