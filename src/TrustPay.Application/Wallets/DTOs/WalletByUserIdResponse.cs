namespace TrustPay.Application.Wallets.DTOs;

public record WalletByUserIdResponse(
        Guid Id,
        Guid UserId,
        decimal Balance,
        string Currency);