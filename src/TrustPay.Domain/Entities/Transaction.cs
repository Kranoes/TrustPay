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
        public DateTime? CompletedAt { get; private set; }
        public string? ErrorMessage { get; private set; }

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

        public Result Complete()
        {
            if (Status != TransactionStatus.Pending)
            {
                return Result.Failure($"Нельзя завершить транзакцию со статусом '{Status}'.");
            }

            Status = TransactionStatus.Completed;
            CompletedAt = DateTime.UtcNow;

            AddDomainEvent(new TransactionCompletedDomainEvent(Id));
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
    }
}