namespace TrustPay.Application.Common.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;
using TrustPay.Domain.Entities;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken = default);
    Task<bool> IsNickNameUnique(string nickName, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(User user);
}