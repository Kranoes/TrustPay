using MediatR;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record UpdateReviewCommand(
    Guid Id,
    string Title,
    string Message,
    int Rating) : IRequest<Result>;
public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, Result>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateReviewCommandHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork,ICurrentUserService currentUserService)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserService.UserId;
        var review = await _reviewRepository.GetByIdAsync(request.Id, cancellationToken);
        if (review is null)
        {
            return Error.NotFound("Review.NotFound", "Отзыв не найден.");
        }
        if (currentUser != review.AuthorId)
        {
            return Error.Forbidden("Review.Forbidden", "У вас нет прав на изменение отзыва.");
        }

        var updateResult = review.Update(request.Title, request.Message, request.Rating);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        _reviewRepository.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}