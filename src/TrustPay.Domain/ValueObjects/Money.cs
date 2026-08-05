using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.ValueObjects
{
    [ComplexType]
    public class Money : ValueObject
    {
        public decimal Amount { get; init; }
        public string Currency { get; init; }
        private Money(decimal amount,string currency)
        {
            Amount = amount;
            Currency = currency;
        }
        private Money()
        {
            Currency = null!;

        }
    public Result<Money>Subtract(Money other)
    {
            if (other.Currency != Currency)
            {
                return Result<Money>.Failure("Нельзя вычитать разные валюты!");

            }
            if (Amount <other.Amount)
            {
                return Result<Money>.Failure("Недостаточно средств.");
            }
            return Result.Success(new Money(Amount - other.Amount, Currency));

    }
    public Money Add(Money other)
        {
            if (other.Currency != Currency)
            { throw new ArgumentException($"Нельзя складывать разные валюты: {Currency} и {other.Currency}"); }
            return new Money(Amount+other.Amount, Currency);
        }
    public static Result<Money>Create(decimal amount,string currency)
        {
            if (amount < 0)
            { return Result.Failure<Money>("Сумма должна быть больше 0"); }
            if(string.IsNullOrEmpty(currency)||currency.Length!=3)
            {
                return Result.Failure<Money>("Код валюты должен состоять из 3 символов (например, RUB, USD).");
            }
            var money = new Money(amount, currency.ToUpper());
            return Result.Success(money);
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
