using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.WalletEvents;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Domain.Entities
{
    public class Wallet : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public WalletStatus Status { get; private set; }
        public Money AvailableBalance { get; private set; } = null!;
        public Money LockedBalance { get; private set; } = null!;
        public uint Version { get; private set; }
        

        private Wallet() { }

        private Wallet(Guid id, Guid userId, Money initialBalance)
            : base(id)
        {
            UserId = userId;
            Status = WalletStatus.Active;
            AvailableBalance = initialBalance;
            LockedBalance = Money.Create(0, initialBalance.Currency).Value;
        }

        public static Result<Wallet> Create(Guid userId, Money initialBalance)
        {
            if (userId == Guid.Empty)
            {
                return Result.Failure<Wallet>("Идентификатор пользователя не может быть пустым.");
            }

            if (initialBalance is null)
            {
                return Result.Failure<Wallet>("Начальный баланс не может быть null.");
            }

            if (initialBalance.Amount < 0)
            {
                return Result.Failure<Wallet>("Начальный баланс не может быть отрицательным.");
            }

            var wallet = new Wallet(Guid.NewGuid(), userId, initialBalance);

            wallet.AddDomainEvent(new WalletCreatedDomainEvent(
                wallet.Id,
                wallet.UserId,
                wallet.AvailableBalance));

            return Result.Success(wallet);
        }

        public bool HasEnoughBalance(Money amount)
        {
            if (!IsSameCurrency(amount))
            {
                return false;
            }

            return AvailableBalance.Amount >= amount.Amount;
        }

        public Result Deposit(Money amount)
        {
            var validateResult = ValidateOperation(amount);
            if (validateResult.IsFailure)
            {
                return validateResult;
            }

            AvailableBalance = AvailableBalance.Add(amount);

            AddDomainEvent(new WalletDepositedDomainEvent(Id, amount));

            return Result.Success();
        }

        public Result Withdraw(Money amount)
        {
            var validateResult = ValidateOperation(amount);
            if (validateResult.IsFailure)
            {
                return validateResult;
            }

            var subtractResult = AvailableBalance.Subtract(amount);
            if (subtractResult.IsFailure)
            {
                return Result.Failure(subtractResult.Error);
            }

            AvailableBalance = subtractResult.Value;

            AddDomainEvent(new WalletWithdrawnDomainEvent(Id, amount));

            return Result.Success();
        }

        public Result LockFunds(Money amount)
        {
            var validateResult = ValidateOperation(amount);
            if (validateResult.IsFailure)
            {
                return validateResult;
            }

            var subtractResult = AvailableBalance.Subtract(amount);
            if (subtractResult.IsFailure)
            {
                return Result.Failure(subtractResult.Error);
            }

            AvailableBalance = subtractResult.Value;
            LockedBalance = LockedBalance.Add(amount);

            AddDomainEvent(new WalletFundsLockedDomainEvent(Id, amount));

            return Result.Success();
        }

        public Result ReleaseLockedFunds(Money amount)
        {
            var validateResult = ValidateOperation(amount);
            if (validateResult.IsFailure)
            {
                return validateResult;
            }

            var lockedSubtractResult = LockedBalance.Subtract(amount);
            if (lockedSubtractResult.IsFailure)
            {
                return Result.Failure("Недостаточно замороженных средств для разблокировки.");
            }

            LockedBalance = lockedSubtractResult.Value;
            AvailableBalance = AvailableBalance.Add(amount);

            AddDomainEvent(new WalletLockedFundsReleasedDomainEvent(Id, amount));

            return Result.Success();
        }

        public Result ConfirmPayment(Money amount)
        {
            var validateResult = ValidateOperation(amount);
            if (validateResult.IsFailure)
            {
                return validateResult;
            }

            var lockedSubtractResult = LockedBalance.Subtract(amount);
            if (lockedSubtractResult.IsFailure)
            {
                return Result.Failure("Недостаточно замороженных средств для подтверждения оплаты.");
            }

            LockedBalance = lockedSubtractResult.Value;

            AddDomainEvent(new WalletPaymentConfirmedDomainEvent(Id, amount));

            return Result.Success();
        }

        public Result Freeze()
        {
            if (Status == WalletStatus.Frozen)
            {
                return Result.Failure("Кошелек уже заморожен.");
            }

            if (Status == WalletStatus.Closed)
            {
                return Result.Failure("Нельзя заморозить закрытый кошелек.");
            }

            var oldStatus = Status;
            Status = WalletStatus.Frozen;

            AddDomainEvent(new WalletStatusChangedDomainEvent(Id, oldStatus, Status));

            return Result.Success();
        }

        public Result Unfreeze()
        {
            if (Status != WalletStatus.Frozen)
            {
                return Result.Failure("Разморозить можно только замороженный кошелек.");
            }

            var oldStatus = Status;
            Status = WalletStatus.Active;

            AddDomainEvent(new WalletStatusChangedDomainEvent(Id, oldStatus, Status));

            return Result.Success();
        }

        public Result Close()
        {
            if (Status == WalletStatus.Closed)
            {
                return Result.Failure("Кошелек уже закрыт.");
            }

            if (AvailableBalance.Amount != 0 || LockedBalance.Amount != 0)
            {
                return Result.Failure("Нельзя закрыть кошелек с ненулевым балансом.");
            }

            var oldStatus = Status;
            Status = WalletStatus.Closed;

            AddDomainEvent(new WalletStatusChangedDomainEvent(Id, oldStatus, Status));

            return Result.Success();
        }

        private Result ValidateOperation(Money amount)
        {
            if (Status != WalletStatus.Active)
            {
                return Result.Failure($"Операция невозможна: кошелек находится в статусе '{Status}'.");
            }

            if (amount is null || amount.Amount <= 0)
            {
                return Result.Failure("Сумма операции должна быть больше нуля.");
            }

            if (!IsSameCurrency(amount))
            {
                return Result.Failure($"Несовпадение валют. Валюта кошелька: '{AvailableBalance.Currency}', передано: '{amount.Currency}'.");
            }

            return Result.Success();
        }

        private bool IsSameCurrency(Money amount)
        {
            return AvailableBalance.Currency == amount.Currency;
        }
    }
}