namespace TrustPay.Application.Orders.DTOs;

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    Guid ExecutorId,
    Guid LotId,
    int Quantity,
    decimal PriceAmount,
    string PriceCurrency,
    string Status,
    DateTime CreatedAt
);