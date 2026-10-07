using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Application.Common.Transactions.Commands.CreateTransfer
{
    public record CreateTransferCommand(
        Guid SenderWalletId,
        Guid ReceiverWalletId,
        Money Amount) : IRequest<Result<TransferResponse>>;

    public class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, Result<TransferResponse>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTransferCommandHandler(
            ITransactionRepository transactionRepository,
            IWalletRepository walletRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TransferResponse>> Handle(
            CreateTransferCommand command,
            CancellationToken cancellationToken)
        {
            if (command.SenderWalletId == command.ReceiverWalletId)
            {
                return Result.Failure<TransferResponse>(
                    Error.Validation("Transfer.SameWallet", "Нельзя перевести средства самому себе."));
            }

            var senderWallet = await _walletRepository.GetByIdAsync(command.SenderWalletId, cancellationToken);
            if (senderWallet is null)
            {
                return Result.Failure<TransferResponse>(
                    Error.NotFound("Wallet.SenderNotFound", "Кошелек отправителя не найден."));
            }

            if (senderWallet.UserId != _currentUserService.UserId)
            {
                return Result.Failure<TransferResponse>(
                    Error.Forbidden("Wallet.AccessDenied", "Вы не являетесь владельцем кошелька-отправителя."));
            }

            var receiverWallet = await _walletRepository.GetByIdAsync(command.ReceiverWalletId, cancellationToken);
            if (receiverWallet is null)
            {
                return Result.Failure<TransferResponse>(
                    Error.NotFound("Wallet.ReceiverNotFound", "Кошелек получателя не найден."));
            }

            var transactionResult = Transaction.CreateTransfer(
                command.SenderWalletId,
                command.ReceiverWalletId,
                command.Amount);

            if (transactionResult.IsFailure)
            {
                return Result.Failure<TransferResponse>(transactionResult.Error);
            }

            var transaction = transactionResult.Value;

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new TransferResponse(transaction.Id, transaction.Status.ToString()));
        }
    }
}