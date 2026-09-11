
using FluentAssertions.Equivalency;
using TrustPay.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace TrustPay.UnitTests.Domain.Entities
{
    public class UserTests
    {
        [Fact]
        public void AddRefreshTokeyn_ShouldAddTokenToUser_WhetDataIsValid()
        {
            var userResult = User.Create("Иван Иванов","ivan@trustpay.com","HashedPassword123");
            var user = userResult.Value;
            var tokenValue = "sample_refresh_token_string";
            var expiresAt = DateTime.UtcNow.AddDays(7);

            user.AddRefreshToken(tokenValue, expiresAt);

            user.RefreshTokens.Should().HaveCount(1);

            var addedToken = user.RefreshTokens.Single();
            addedToken.Token.Should().Be(tokenValue);
            addedToken.ExpireAt.Should().Be(expiresAt);
            addedToken.IsExpired.Should().BeFalse();


        }
        [Fact]
        public void RevokeRefreshToken_ShouldMarkTokenAsRevoked_WhenTokenExists()
        {
            var userResult = User.Create("Ivan ivanov", "ivan@trustpay.com", "HashedPassword123");
            var user = userResult.Value;
            var tokenValue = "token_to_revoke";
            user.AddRefreshToken(tokenValue, DateTime.UtcNow.AddDays(7));

            user.RevokeRefreshToken(tokenValue);

            var token = user.RefreshTokens.Single(t => t.Token == tokenValue);
            token.IsExpired.Should().BeTrue();
        }
    }
}
