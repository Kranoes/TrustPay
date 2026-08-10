using System;
using System.Collections.Generic;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.UserEvents;

namespace TrustPay.Domain.Entities
{
    public class User : AggregateRoot<Guid>
    {
        private readonly List<Lot> _lots = new();

        public string UserName { get; private set; } = null!;
        public string UserEmail { get; private set; } = null!;
        public double AvgRating { get; private set; }
        public int CountOfValuations { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public UserRole Role { get; private set; }

        public Wallet? Wallet { get; private set; }

        public IReadOnlyCollection<Lot> Lots => _lots.AsReadOnly();

        private User() { }

        private User(Guid id, string email, string nickName, UserRole role)
            : base(id)
        {
            UserEmail = email;
            UserName = nickName;
            Role = role;
            AvgRating = 0;
            CountOfValuations = 0;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<User> Create(string email, string nickName, UserRole role = UserRole.User)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure<User>("Некорректный email.");
            }

            if (string.IsNullOrWhiteSpace(nickName))
            {
                return Result.Failure<User>("Некорректный никнейм.");
            }

            var user = new User(
                Guid.NewGuid(),
                email.Trim(),
                nickName.Trim(),
                role);

            user.AddDomainEvent(new UserCreatedDomainEvent(
                user.Id,
                user.UserEmail,
                user.UserName,
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

      
    }
}