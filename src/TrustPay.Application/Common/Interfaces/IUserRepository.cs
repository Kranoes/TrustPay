using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Entities;

namespace TrustPay.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken = default);
        Task<bool> IsNickNameUnique(string nickName, CancellationToken cancellationToken = default);
        void Update(User user);
        void Delete(User user);
    }
}
