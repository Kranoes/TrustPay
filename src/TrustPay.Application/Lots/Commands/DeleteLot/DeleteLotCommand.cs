namespace TrustPay.Application.Lots.Commands.DeleteLot;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record DeleteLotCommand(Guid Id) : IRequest<Result>;

public class DeleteLotCommandHandler : IRequestHandler<DeleteLotCommand, Result>
{
    private readonly ILotRepository _lotRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    public DeleteLotCommandHandler(ILotRepository lotRepository, IUnitOfWork unitOfWork,ICurrentUserService currentUserService)
    {
        _lotRepository = lotRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteLotCommand request, CancellationToken cancellationToken)
    {
        var lot = await _lotRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lot is null)
        {
            return Error.NotFound("Lot.NotFound", "Лот не найден.");
        }
        var currentUser = _currentUserService.UserId;
        if (lot.UserId != currentUser && !_currentUserService.IsAdmin)
        {
            return Result.Failure("Только создатель лота или админ может удалить лот.");
        }

        _lotRepository.Delete(lot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}