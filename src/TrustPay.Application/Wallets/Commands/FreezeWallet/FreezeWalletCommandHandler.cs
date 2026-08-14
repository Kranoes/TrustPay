using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
namespace TrustPay.Application.Wallets.Commands.FreezeWallet
{
    public class FreezeWalletCommandHandler : IRequestHandler<FreezeWalletCommand,Result>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        public FreezeWalletCommandHandler(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
        {
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result> Handle(FreezeWalletCommand command,CancellationToken cancellationToken= default)
        {
            var wallet = await _walletRepository.GetByIdAsync(command.WalletId, cancellationToken);
            if (wallet is null)
            {
                return Result.Failure($"Кошелек с ID '{command.WalletId}' не найден");

            }
            Result freezeResult = wallet.Freeze();
            if(freezeResult.IsFailure)
            {
                return freezeResult;
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
