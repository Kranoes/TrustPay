namespace TrustPay.Application.Reviews.Commands.DeleteReview;

using FluentValidation;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record DeleteReviewCommand(Guid Id) : IRequest<Result>;


public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Result>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteReviewCommandHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork,ICurrentUserService currentUserService)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserService.UserId;
        var isAdmin = _currentUserService.IsAdmin;
        var review = await _reviewRepository.GetByIdAsync(request.Id, cancellationToken);
        if (review is null)
        {
            return Error.NotFound("Review.NotFound", "Отзыв не найден.");
        }
        if (currentUser != review.AuthorId || !isAdmin)
        {
            return Error.Forbidden("Review.Forbidden","Недостаточно прав чтобы удалить отзыв.");
        }


            _reviewRepository.Delete(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}