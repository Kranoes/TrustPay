namespace TrustPay.Application.Reviews.Commands.CreateReview;

using FluentValidation;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;

public record CreateReviewCommand(
    Guid OrderId,
    string Title,
    string Message,
    int Rating) : IRequest<Result<Guid>>;



public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<Guid>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReviewCommandHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var existingReview = await _reviewRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingReview is not null)
        {
            return Result.Failure<Guid>(Error.Conflict("Review.AlreadyExists", "Отзыв к этому заказу уже существует."));
        }

        var reviewResult = Review.Create(request.OrderId, request.Title, request.Message, request.Rating);
        if (reviewResult.IsFailure)
        {
            return Result.Failure<Guid>(reviewResult.Error);
        }

        await _reviewRepository.AddAsync(reviewResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(reviewResult.Value.Id);
    }
}