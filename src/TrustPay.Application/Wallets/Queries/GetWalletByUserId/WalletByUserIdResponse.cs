namespace TrustPay.Application.Wallets.Queries.WalletByUserIdResponse;

public record WalletByUserIdResponse(
        Guid Id,
        Guid UserId,
        decimal Balance,
        string Currency);