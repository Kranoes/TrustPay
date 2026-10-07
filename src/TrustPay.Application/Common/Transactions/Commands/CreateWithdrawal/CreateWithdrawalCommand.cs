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

namespace TrustPay.Application.Common.Transactions.Commands.CreateWithdrawal
{
    public record CreateWithdrawalCommand(Guid SenderWalletId, Money Amount) : IRequest<Result<WithdrawalResponse>>;

    public class CreateWithdrawalCommandHandler : IRequestHandler<CreateWithdrawalCommand, Result<WithdrawalResponse>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWithdrawalCommandHandler(
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

        public async Task<Result<WithdrawalResponse>> Handle(
            CreateWithdrawalCommand command,
            CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(command.SenderWalletId, cancellationToken);
            if (wallet is null)
            {
                return Result.Failure<WithdrawalResponse>(
                    Error.NotFound("Wallet.NotFound", "Кошелек отправителя не найден."));
            }

            if (wallet.UserId != _currentUserService.UserId)
            {
                return Result.Failure<WithdrawalResponse>(
                    Error.Forbidden("Wallet.AccessDenied", "У вас нет прав на совершение операций с этим кошельком."));
            }

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