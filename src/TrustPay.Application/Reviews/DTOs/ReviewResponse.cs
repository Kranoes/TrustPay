namespace TrustPay.Application.Reviews.DTOs;

public record ReviewResponse(
    Guid Id,
    Guid OrderId,
    string Title,
    string Message,
    int Rating,
    DateTime CreatedAt);