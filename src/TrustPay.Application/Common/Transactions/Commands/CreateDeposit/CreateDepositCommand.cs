using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Application.Common.Transactions.Commands.CreateDeposit
{
    public record CreateDepositCommand(Guid ReceiverWalletId, Money Amount) : IRequest<Result<DepositResponse>>;

    public class CreateDepositCommandHandler : IRequestHandler<CreateDepositCommand, Result<DepositResponse>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWalletRepository _walletRepository;

        public CreateDepositCommandHandler(
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork,
            IPaymentGatewayService paymentGatewayService,
            IWalletRepository walletRepository)
        {
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _paymentGatewayService = paymentGatewayService;
            _walletRepository = walletRepository;
        }

        public async Task<Result<DepositResponse>> Handle(
            CreateDepositCommand command,
            CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(command.ReceiverWalletId, cancellationToken);
            if (wallet is null)
            {
                return Result<DepositResponse>.Failure("Кошелек не найден.");
            }
            var transactionResult = Transaction.CreateDeposit(
                command.ReceiverWalletId,
                command.Amount);

            if (transactionResult.IsFailure)
            {
                return Result.Failure<DepositResponse>(transactionResult.Error);
            }

            var transaction = transactionResult.Value;

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            try
            {
                var paymentResult = await _paymentGatewayService.CreatePaymentFormAsync(
                    transaction.Id,
                    transaction.Amount,
                    cancellationToken);
                var setexternalIdResult = transaction.SetExternalPaymentId(paymentResult.ExternalPaymentId);
                if (setexternalIdResult.IsFailure)
                {
                    return Result<DepositResponse>.Failure(setexternalIdResult.Error);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(new DepositResponse(transaction.Id, paymentResult.PaymentUrl));

            }
            catch (Exception ex) 
            {
                transaction.Fail($"Ошибка связи с платежным шлюзом: {ex.Message}");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<DepositResponse>.Failure("Не удалось зарегистрировать платеж в банке.");
            }
            }
    }
}