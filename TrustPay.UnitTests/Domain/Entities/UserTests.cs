
using FluentAssertions.Equivalency;
using TrustPay.Domain.Entities;
using Xunit;
using FluentAssertions;
using TrustPay.Domain.Common;
using System.Text;

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
            addedToken.TokenHash.Should().Be(tokenValue);
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

            var result = user.RevokeRefreshToken(tokenValue);

            result.IsSuccess.Should().BeTrue();
            var token = user.RefreshTokens.Single(t => t.TokenHash == tokenValue);
            token.IsRevoked.Should().BeTrue();
        }
        
        [Fact]
        public void AddRefreshTokeyn_ShouldRemoveRevokedTokens_WhenTheyAreOverCounted()
        {
            var expectedCountOfTokens = 30;
            var currentCountOfTokens = 35;
            var validTokenHash = "valid-tokenHash-mok";
            var listOfTokensHashShouldBeDeleted = new List<string>();
            var listOfTokenHashShouldBe = new List<string>();
            var user = User.Create("Ivan ivanov", "ivan@trustpay.com", "HashedPassword123").Value;
            user.AddRefreshToken(validTokenHash, new DateTime(2028,10,10));
            for (int i = 0; i < currentCountOfTokens; i++)
            {
                if (i < currentCountOfTokens - expectedCountOfTokens)
                {
                    listOfTokensHashShouldBeDeleted.Add(validTokenHash+i);
                }
                if (i >=   currentCountOfTokens - expectedCountOfTokens)
                {
                    listOfTokenHashShouldBe.Add(validTokenHash+i);
                }
                user.AddRefreshToken(validTokenHash + i, new DateTime(2028, 10, 10));
                user.RevokeRefreshToken(validTokenHash+i);
                
            }
            user.RefreshTokens.Select(t => t.TokenHash).ToList().Should().Contain(validTokenHash);
            user.RefreshTokens.Count.Should().Be(expectedCountOfTokens + 1);
            foreach (var tokenHash in listOfTokensHashShouldBeDeleted)
            {
                user.RefreshTokens.Select(t => t.TokenHash).ToList().Should().NotContain(tokenHash);
            }
            foreach (var tokenHash in listOfTokenHashShouldBe)
            {
                user.RefreshTokens.Select(t=>t.TokenHash).ToList().Should().Contain(tokenHash);
            }
            

        }
        
    }
}
