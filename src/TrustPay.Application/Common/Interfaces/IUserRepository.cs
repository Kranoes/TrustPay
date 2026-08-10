using TrustPay.Domain.Entities;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(User user);

    Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken = default);
    Task<bool> IsNickNameUnique(string nickName, CancellationToken cancellationToken = default);
}