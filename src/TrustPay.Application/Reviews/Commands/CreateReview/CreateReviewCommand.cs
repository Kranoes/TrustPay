namespace TrustPay.Application.Reviews.Commands.CreateReview;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

public record CreateReviewCommand(
    Guid OrderId,
    string Title,
    string Message,
    int Rating) : IRequest<Result<Guid>>;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<Guid>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Order.NotFound", "Заказ не найден."));
        }

        if (order.CustomerId != currentUserId)
        {
            return Result.Failure<Guid>(Error.Forbidden("Review.Forbidden", "Вы не можете оставить отзыв к чужому заказу."));
        }

        if (order.Status != OrderStatus.Completed)
        {
            return Result.Failure<Guid>(Error.Failure("Review.OrderNotCompleted", "Отзыв можно оставить только к завершенному заказу."));
        }

        var existingReview = await _reviewRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingReview is not null)
        {
            return Result.Failure<Guid>(Error.Conflict("Review.AlreadyExists", "Отзыв к этому заказу уже существует."));
        }

        var reviewResult = Review.Create(
            request.OrderId,
            currentUserId,
            order.ExecutorId,
            request.Title,
            request.Message,
            request.Rating);

        if (reviewResult.IsFailure)
        {
            return Result.Failure<Guid>(reviewResult.Error);
        }

        await _reviewRepository.AddAsync(reviewResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(reviewResult.Value.Id);
    }
}