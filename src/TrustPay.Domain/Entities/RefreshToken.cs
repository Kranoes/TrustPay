using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; init; }
    public string Token { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpireAt { get; init; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;
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
        if (DateTime.UtcNow >= expireAt)
        {
            return Result<RefreshToken>.Failure("Срок действия токена не может быть в прошлом.");
        }

        var refreshToken = new RefreshToken(token, DateTime.UtcNow, expireAt, userId);
        return Result<RefreshToken>.Success(refreshToken);
    }

    public Result Revoke()
    {
        if (IsRevoked)
        {
            return Result.Failure("Токен уже был отозван.");
        }
        if (IsExpired)
        {
            return Result.Failure("Невозможно отозвать истекший токен.");
        }

        RevokedAt = DateTime.UtcNow;
        return Result.Success();
    }
}