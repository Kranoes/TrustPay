using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; init; } 
        public string Token { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
        public DateTime ExpireAt { get; init; }
        public Guid UserId { get; init; }
        public bool IsExpired => DateTime.UtcNow >= ExpireAt;
        private RefreshToken() { }
        private RefreshToken(string token, DateTime createdAt, DateTime expireAt, Guid userId)
        {
            Id = Guid.NewGuid();
            Token = token;
            CreatedAt = createdAt;
            ExpireAt = expireAt;
            UserId = userId;
        }
        public static Result<RefreshToken> Create(string token, DateTime expireAt, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Result<RefreshToken>.Failure("Неверный формат токена.");
            }
            if (DateTime.UtcNow >=expireAt )
            {
                return Result<RefreshToken>.Failure("Неверная дата регистрации токена.");
            }
            
            var refreshToken  = new RefreshToken(token, DateTime.UtcNow, expireAt, userId);
            return Result<RefreshToken>.Success(refreshToken);
        }
    }
}
