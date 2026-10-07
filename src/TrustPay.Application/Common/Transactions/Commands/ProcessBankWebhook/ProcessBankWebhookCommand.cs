using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

namespace TrustPay.Application.Common.Transactions.Commands.ProcessBankWebhook
{
    public record ProcessBankWebhookCommand(
        Guid TransactionId,
        bool IsSuccess,
        string? FailureReason,
        string? ExternalPaymentId) : IRequest<Result>;

    public class ProcessBankWebhookCommandHandler : IRequestHandler<ProcessBankWebhookCommand, Result>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessBankWebhookCommandHandler(
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ProcessBankWebhookCommand command,
            CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetByIdAsync(command.TransactionId, cancellationToken);
            if (transaction is null)
            {
                return Result.Failure(
                    Error.NotFound("Transaction.NotFound", "Транзакция не найдена."));
            }

            if (transaction.Status == TransactionStatus.Completed)
            {
                return Result.Success();
            }

            if (transaction.Status == TransactionStatus.Failed)
            {
                return Result.Failure(
                    Error.Conflict("Transaction.InvalidStatus", "Транзакция находится в некорректном статусе."));
            }

            if (command.IsSuccess)
            {
                transaction.Complete(transaction.PaymentSource, transaction.ReceiverWalletId, transaction.Amount);
            }
            else
            {
                transaction.Fail(command.FailureReason!);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}