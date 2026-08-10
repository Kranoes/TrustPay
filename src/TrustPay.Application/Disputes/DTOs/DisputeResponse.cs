namespace TrustPay.Application.Disputes.DTOs;

using TrustPay.Domain.Enums;

public record DisputeResponse(
    Guid Id,
    Guid CustomerId,
    Guid ExecutorId,
    Guid? ArbitratorId,
    string Reason,
    DisputeStatus Status,
    DateTime CreatedAt
);