using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Application.Common.Transactions.Commands.CreateWithdrawal
{
    public record CreateWithdrawalCommand(Guid SenderWalletId, Money Amount) : IRequest<Result<WithdrawalResponse>>;

    public class CreateWithdrawalCommandHandler : IRequestHandler<CreateWithdrawalCommand, Result<WithdrawalResponse>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWithdrawalCommandHandler(
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<WithdrawalResponse>> Handle(
            CreateWithdrawalCommand command,
            CancellationToken cancellationToken)
        {
            var transactionResult = Transaction.CreateWithdrawal(
                command.SenderWalletId,
                command.Amount);

            if (transactionResult.IsFailure)
            {
                return Result.Failure<WithdrawalResponse>(transactionResult.Error);
            }

            var transaction = transactionResult.Value;

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new WithdrawalResponse(transaction.Id, transaction.Status.ToString()));
        }
    }
}