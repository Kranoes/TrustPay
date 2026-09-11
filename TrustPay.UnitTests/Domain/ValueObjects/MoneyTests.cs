namespace TrustPay.UnitTests.Domain.ValueObjects;

using FluentAssertions;
using TrustPay.Domain.ValueObjects;
using Xunit;

public class MoneyTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenAmountAndCurrencyAreValid()
    {
        var amount = 100m;
        var currency = "RUB";

        var result = Money.Create(amount, currency);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(amount);
        result.Value.Currency.Should().Be("RUB");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenAmountIsNegative()
    {
        var result = Money.Create(-100m, "RUB");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Сумма должна быть больше 0");
    }

    [Fact]
    public void Add_ShouldIncreaseAmount_WhenCurrenciesMatch()
    {
        var money1 = Money.Create(100m, "RUB").Value;
        var money2 = Money.Create(50m, "RUB").Value;

        var result = money1.Add(money2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(150m);
        result.Value.Currency.Should().Be("RUB");
    }

    [Fact]
    public void Add_ShouldReturnFailure_WhenCurrenciesMismatch()
    {
        var money1 = Money.Create(100m, "RUB").Value;
        var money2 = Money.Create(50m, "USD").Value;

        var result = money1.Add(money2);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Нельзя складывать разные валюты");
    }

    [Fact]
    public void Subtract_ShouldReturnSuccess_WhenFundsAreSufficient()
    {
        var money1 = Money.Create(100m, "RUB").Value;
        var money2 = Money.Create(40m, "RUB").Value;

        var result = money1.Subtract(money2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(60m);
    }

    [Fact]
    public void Subtract_ShouldReturnFailure_WhenInsufficientFunds()
    {
        var money1 = Money.Create(30m, "RUB").Value;
        var money2 = Money.Create(50m, "RUB").Value;

        var result = money1.Subtract(money2);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Недостаточно средств.");
    }
}