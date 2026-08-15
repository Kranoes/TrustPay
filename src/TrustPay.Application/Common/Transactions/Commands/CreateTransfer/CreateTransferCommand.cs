using MediatR;
using TrustPay.Application.Common.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;

        public CreateTransferCommandHandler(
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TransferResponse>> Handle(
            CreateTransferCommand command,
            CancellationToken cancellationToken)
        {
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