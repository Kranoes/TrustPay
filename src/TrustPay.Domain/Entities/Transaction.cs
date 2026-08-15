using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.TransactionsEvents;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Domain.Entities
{
    public class Transaction : AggregateRoot<Guid>
    {
        public Guid? SenderWalletId { get; private set; }
        public Guid? ReceiverWalletId { get; private set; }
        public Money Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public TransactionStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string? PaymentSource { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? ExternalPaymentId { get; private set; }

        private Transaction() { }

        private Transaction(
            Guid id,
            Guid? senderWalletId,
            Guid? receiverWalletId,
            Money amount,
            TransactionType type)
            : base(id)
        {
            SenderWalletId = senderWalletId;
            ReceiverWalletId = receiverWalletId;
            Amount = amount;
            Type = type;
            Status = TransactionStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Transaction> CreateTransfer(
            Guid senderWalletId,
            Guid receiverWalletId,
            Money amount)
        {
            if (senderWalletId == Guid.Empty)
            {
                return Result.Failure<Transaction>("Идентификатор кошелька отправителя не может быть пустым.");
            }

            if (receiverWalletId == Guid.Empty)
            {
                return Result.Failure<Transaction>("Идентификатор кошелька получателя не может быть пустым.");
            }

            if (senderWalletId == receiverWalletId)
            {
                return Result.Failure<Transaction>("Нельзя совершить перевод на один и тот же кошелек.");
            }

            if (amount == null || amount.Amount <= 0)
            {
                return Result.Failure<Transaction>("Сумма перевода должна быть больше нуля.");
            }

            var transaction = new Transaction(
                Guid.NewGuid(),
                senderWalletId,
                receiverWalletId,
                amount,
                TransactionType.Transfer);

            transaction.AddDomainEvent(new TransactionCreatedDomainEvent(
                transaction.Id,
                transaction.SenderWalletId,
                transaction.ReceiverWalletId,
                transaction.Amount));

            return Result.Success(transaction);
        }
        public static Result<Transaction> CreateDeposit(Guid receiverWalletId,Money amount)
        {
            if (receiverWalletId == Guid.Empty)
            {
                return Result<Transaction>.Failure("Некорректный ID кошелька.");
            }
            if (amount is null || amount.Amount <= 0)
            {
                return Result<Transaction>.Failure("Некорректная сумма депозита.");
            }
            
            var transaction = new Transaction(
            
                 Guid.NewGuid(),
                 null,
                 receiverWalletId,
                 amount,
                 TransactionType.Deposit
            );
            transaction.AddDomainEvent(new TransactionCreatedDomainEvent(
                transaction.Id,
                null,
                transaction.ReceiverWalletId,
                transaction.Amount
                ));
            return Result.Success(transaction);

        }
        public static Result<Transaction> CreateWithdrawal(Guid senderWalletId,Money amount)
        {
            if (senderWalletId == Guid.Empty)
            {
                return Result.Failure<Transaction>("Некорректный ID.");
            }
            if (amount is null || amount.Amount <= 0)
            {
                return Result.Failure<Transaction>("Некорректная сумма вывода.");
            }
            var transaction = new Transaction(
                Guid.NewGuid(),
                senderWalletId,
                null,
                amount,
                TransactionType.Withdrawal
                );
            transaction.AddDomainEvent(new TransactionCreatedDomainEvent(
                 transaction.Id,
                 transaction.SenderWalletId,
                 null,
                 transaction.Amount
                ));
            return Result.Success(transaction);
        }
        public Result Complete(string? paymentSource,Guid? walletId,Money amount)
        {
            if (Status != TransactionStatus.Pending)
            {
                return Result.Failure($"Нельзя завершить транзакцию со статусом '{Status}'.");
            }

            Status = TransactionStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            PaymentSource = paymentSource;
            AddDomainEvent(new TransactionCompletedDomainEvent(Id,walletId,amount,paymentSource));
            return Result.Success();
        }

        public Result Fail(string errorMessage)
        {
            if (Status != TransactionStatus.Pending)
            {
                return Result.Failure($"Нельзя перевести в статус 'Ошибка' транзакцию со статусом '{Status}'.");
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return Result.Failure("Сообщение об ошибке не может быть пустым.");
            }

            Status = TransactionStatus.Failed;
            ErrorMessage = errorMessage;

            AddDomainEvent(new TransactionFailedDomainEvent(Id, errorMessage));
            return Result.Success();
        }
        public Result SetExternalPaymentId(string externalPaymentId)
        {
            if (string.IsNullOrWhiteSpace(externalPaymentId))
            {
                return Result.Failure("Внешний ID платежа не может быть пустым.");
            }
            ExternalPaymentId = externalPaymentId;
            return Result.Success();
        }

    }
}