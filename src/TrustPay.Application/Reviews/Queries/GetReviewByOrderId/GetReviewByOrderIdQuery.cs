namespace TrustPay.Application.Reviews.Queries.GetByOrderId;

using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Reviews.DTOs;
using TrustPay.Domain.Common;

public record GetReviewByOrderIdQuery(Guid OrderId) : IRequest<Result<ReviewResponse>>;

public class GetReviewByOrderIdQueryHandler : IRequestHandler<GetReviewByOrderIdQuery, Result<ReviewResponse>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewByOrderIdQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<ReviewResponse>> Handle(GetReviewByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (review is null)
        {
            return Result.Failure<ReviewResponse>(Error.NotFound("Review.NotFound", "Отзыв для данного заказа не найден."));
        }

        var response = new ReviewResponse(
            review.Id,
            review.OrderId,
            review.Title,
            review.Message,
            review.Rating,
            review.CreatedAt);

        return Result.Success(response);
    }
}