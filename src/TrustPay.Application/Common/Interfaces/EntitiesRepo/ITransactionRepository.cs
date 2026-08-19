namespace TrustPay.Application.Common.Interfaces.EntitiesRepo;

using TrustPay.Domain.Entities;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Transaction>> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken);
    Task<(List<Transaction> Items, int TotalCount)> GetPagedByWalletIdAsync(
        Guid walletId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}