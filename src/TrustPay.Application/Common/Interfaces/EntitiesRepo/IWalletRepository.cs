using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Entities;

namespace TrustPay.Application.Common.Interfaces.EntitiesRepo
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
        void Update(Wallet wallet);
        void Delete(Wallet wallet);
    }
}
