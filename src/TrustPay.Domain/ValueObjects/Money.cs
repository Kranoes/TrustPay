using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.ValueObjects
{
    [ComplexType]
    public class Money : ValueObject
    {
        public decimal Amount { get; init; }
        public string Currency { get; init; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        private Money()
        {
            Currency = null!;
        }

        public static Result<Money> Create(decimal amount, string currency)
        {
            if (amount < 0)
            {
                return Result.Failure<Money>(
                    Error.Validation("Money.NegativeAmount", "Сумма не может быть отрицательной."));
            }

            if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            {
                return Result.Failure<Money>(
                    Error.Validation("Money.InvalidCurrency", "Код валюты должен состоять из 3 символов (например, RUB, USD)."));
            }

            var money = new Money(amount, currency.Trim().ToUpper());
            return Result.Success(money);
        }

        public Result<Money> Add(Money other)
        {
            if (other.Currency != Currency)
            {
                return Result.Failure<Money>(
                    Error.Validation("Money.CurrencyMismatch", $"Нельзя складывать разные валюты: {Currency} и {other.Currency}."));
            }

            return Result.Success(new Money(Amount + other.Amount, Currency));
        }

        public Result<Money> Subtract(Money other)
        {
            if (other.Currency != Currency)
            {
                return Result.Failure<Money>(
                    Error.Validation("Money.CurrencyMismatch", $"Нельзя вычитать разные валюты: {Currency} и {other.Currency}."));
            }

            if (Amount < other.Amount)
            {
                return Result.Failure<Money>(
                    Error.Validation("Money.InsufficientFunds", "Недостаточно средств."));
            }

            return Result.Success(new Money(Amount - other.Amount, Currency));
        }

        public static Result<Money> Zero(string currency)
        {
            return Create(0, currency);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}