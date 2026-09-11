namespace TrustPay.UnitTests.Domain.Entities;

using FluentAssertions;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;
using Xunit;

public class WalletTests
{
    [Fact]
    public void Deposit_ShouldIncreaseBalance_WhenAmountIsValid()
    {
        var userId = Guid.NewGuid();
        var initialBalance = Money.Create(0m, "RUB").Value;
        var wallet = Wallet.Create(userId, initialBalance).Value;
        var depositAmount = Money.Create(500m, "RUB").Value;

        var result = wallet.Deposit(depositAmount);

        result.IsSuccess.Should().BeTrue();
        wallet.AvailableBalance.Amount.Should().Be(500m);
    }

    [Fact]
    public void Withdraw_ShouldDecreaseBalance_WhenFundsAreSufficient()
    {
        var userId = Guid.NewGuid();
        var initialBalance = Money.Create(1000m, "RUB").Value;
        var wallet = Wallet.Create(userId, initialBalance).Value;
        var withdrawAmount = Money.Create(400m, "RUB").Value;

        var result = wallet.Withdraw(withdrawAmount);

        result.IsSuccess.Should().BeTrue();
        wallet.AvailableBalance.Amount.Should().Be(600m);
    }

    [Fact]
    public void Withdraw_ShouldReturnFailure_WhenInsufficientFunds()
    {
        var userId = Guid.NewGuid();
        var initialBalance = Money.Create(50m, "RUB").Value;
        var wallet = Wallet.Create(userId, initialBalance).Value;
        var withdrawAmount = Money.Create(100m, "RUB").Value;

        var result = wallet.Withdraw(withdrawAmount);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Недостаточно средств.");
        wallet.AvailableBalance.Amount.Should().Be(50m);
    }
}