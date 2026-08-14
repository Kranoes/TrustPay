namespace TrustPay.Application.Reviews.Queries.GetById;

using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Reviews.DTOs;
using TrustPay.Domain.Common;

public record GetReviewByIdQuery(Guid Id) : IRequest<Result<ReviewResponse>>;

public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, Result<ReviewResponse>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewByIdQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<ReviewResponse>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.Id, cancellationToken);
        if (review is null)
        {
            return Result.Failure<ReviewResponse>(Error.NotFound("Review.NotFound", "Отзыв не найден."));
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