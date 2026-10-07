using System;
using System.Collections.Generic;
using System.Linq;
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
        public DateTime? LastNickNameChangedAt { get; private set; }
        public UserRole Role { get; private set; }

        private readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
        private const int MaxActiveTokens = 5;
        private const int NickNameChangeIntervalDays = 30;

        public DateTime? NextAllowedNickNameChangeAt =>
            LastNickNameChangedAt?.AddDays(NickNameChangeIntervalDays);

        private User() { }

        private User(Guid id, string email, string nickName, string passwordHash, UserRole role)
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
                return Error.Validation("User.EmailIsEmpty", "Email не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(nickName))
            {
                return Error.Validation("User.NickNameIsEmpty", "Никнейм не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return Error.Validation("User.PasswordHashIsEmpty", "Хэш пароля не может быть пустым.");
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

            return user;
        }

        
        public Result UpdateProfile(string newNickName)
        {
            if (string.IsNullOrWhiteSpace(newNickName))
            {
                return Error.Validation("User.NickNameIsEmpty", "Никнейм не может быть пустым.");
            }

            var trimmedNickName = newNickName.Trim();

            if (string.Equals(Name, trimmedNickName, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Success();
            }

            if (LastNickNameChangedAt.HasValue &&
                DateTime.UtcNow < LastNickNameChangedAt.Value.AddDays(NickNameChangeIntervalDays))
            {
                return Error.Conflict(
                    "User.NickNameChangeCooldown",
                    $"Никнейм можно менять не чаще одного раза в {NickNameChangeIntervalDays} дней. " +
                    $"Следующая смена доступна с {NextAllowedNickNameChangeAt:g} UTC.");
            }

            Name = trimmedNickName;
            LastNickNameChangedAt = DateTime.UtcNow;

            return Result.Success();
        }

        
        public Result ChangeEmail(string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
            {
                return Error.Validation("User.EmailIsEmpty", "Email не может быть пустым.");
            }

            var trimmedEmail = newEmail.Trim();

            if (string.Equals(Email, trimmedEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Success();
            }

            Email = trimmedEmail;

            return Result.Success();
        }

        public Result ChangeRole(UserRole newRole)
        {
            if (Role == newRole)
            {
                return Error.Conflict("User.RoleUnchanged", "Пользователь уже имеет эту роль.");
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
                return result.Error;
            }

            _refreshTokens.RemoveAll(t => !t.IsActive);

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
                return Error.NotFound("User.TokenNotFound", "Токен не найден.");
            }

            return refreshToken.Revoke();
        }
    }
}