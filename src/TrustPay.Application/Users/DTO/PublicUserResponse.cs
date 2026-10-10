namespace TrustPay.Application.Users.DTO;

public record PublicUserResponse(
    Guid Id,
    string NickName,
    double AvgRating,
    int CountOfValuations,
    DateTime CreatedAt);
