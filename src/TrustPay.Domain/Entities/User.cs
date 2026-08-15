using System;
using System.Collections.Generic;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.UserEvents;

namespace TrustPay.Domain.Entities
{
    public class User : AggregateRoot<Guid>
    {

        public string Name { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public double AvgRating { get; private set; }
        public string PasswordHash { get; private set; } = null!;
        public int CountOfValuations { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public UserRole Role { get; private set; }
        private readonly List<RefreshToken> _refreshTokens=new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
        private const int MaxActiveTokens = 5;


        private User() { }

        private User(Guid id, string email, string nickName, string passwordHash ,UserRole role)
            : base(id)
        {
            Email = email;
            Name = nickName;
            PasswordHash = passwordHash;
            Role = role;
            AvgRating = 0;
            CountOfValuations = 0;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<User> Create(string email, string nickName, string passwordHash, UserRole role = UserRole.User)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure<User>("Некорректный email.");
            }

            if (string.IsNullOrWhiteSpace(nickName))
            {
                return Result.Failure<User>("Некорректный никнейм.");
            }
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return Result.Failure<User>("Некорректный хэш пароля.");
            }
            var user = new User(
                Guid.NewGuid(),
                email.Trim(),
                nickName.Trim(),
                passwordHash.Trim(),
                role);

            user.AddDomainEvent(new UserCreatedDomainEvent(
                user.Id,
                user.Email,
                user.Name,
                user.Role));

            return Result.Success(user);
        }

        public Result ChangeRole(UserRole newRole)
        {
            if (Role == newRole)
            {
                return Result.Failure("Пользователь уже имеет эту роль.");
            }

            var oldRole = Role;
            Role = newRole;

            AddDomainEvent(new UserRoleChangedDomainEvent(Id, oldRole, newRole));

            return Result.Success();
        }
        public Result AddRefreshToken(string token, DateTime expireAt)
        {
            var result = RefreshToken.Create(token, expireAt, Id);
            if (result.IsFailure)
            {
                return Result.Failure(result.Error);
            }
            _refreshTokens.RemoveAll(t => t.IsExpired);
            if (_refreshTokens.Count >= MaxActiveTokens)
            {
                var oldestToken = _refreshTokens.OrderBy(t => t.CreatedAt).First();
                _refreshTokens.Remove(oldestToken);
            }
            _refreshTokens.Add(result.Value);
            return Result.Success();
        }
        public Result RevokeRefreshToken(string token)
        {
            var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
            if (refreshToken is null)
            {
                return Result.Failure("Токен не найден.");
            }

            _refreshTokens.Remove(refreshToken);
            return Result.Success();
        }

    }
}