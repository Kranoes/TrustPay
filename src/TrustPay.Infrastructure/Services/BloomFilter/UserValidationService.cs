namespace TrustPay.Infrastructure.Services;

using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using TrustPay.Application.Common.Interfaces.BloomFilter;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;

public class UserValidationService : IUserValidationService
{
    private readonly IDatabase _redisDb;
    private readonly IUserRepository _userRepository;

    private const string EmailBloomKey = "trustpay:emails:bloom";
    private const string NickNameBloomKey = "trustpay:nicknames:bloom";

    public UserValidationService(IConnectionMultiplexer redis, IUserRepository userRepository)
    {
        _redisDb = redis.GetDatabase();
        _userRepository = userRepository;
    }

    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var existsInBloom = (bool)await _redisDb.ExecuteAsync("BF.EXISTS", EmailBloomKey, normalizedEmail);

        if (!existsInBloom)
        {
            return false;
        }

        var isUniqueInDb = await _userRepository.IsEmailUniqueAsync(normalizedEmail, cancellationToken);

        return !isUniqueInDb;
    }

    public async Task<bool> IsNickNameTakenAsync(string nickName, CancellationToken cancellationToken = default)
    {
        var normalizedNickName = nickName.Trim().ToLowerInvariant();

        var existsInBloom = (bool)await _redisDb.ExecuteAsync("BF.EXISTS", NickNameBloomKey, normalizedNickName);

        if (!existsInBloom)
        {
            return false;
        }

        var isUniqueInDb = await _userRepository.IsNickNameUniqueAsync(normalizedNickName, cancellationToken);

        return !isUniqueInDb;
    }

    public async Task RegisterUserEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        await _redisDb.ExecuteAsync("BF.ADD", EmailBloomKey, normalizedEmail);
    }

    public async Task RegisterUserNickNameAsync(string nickName, CancellationToken cancellationToken = default)
    {
        var normalizedNickName = nickName.Trim().ToLowerInvariant();
        await _redisDb.ExecuteAsync("BF.ADD", NickNameBloomKey, normalizedNickName);
    }
}