namespace TrustPay.Application.Lots.Commands.DeleteLot;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;

public record DeleteLotCommand(Guid Id) : IRequest<Result>;

public class DeleteLotCommandHandler : IRequestHandler<DeleteLotCommand, Result>
{
    private readonly ILotRepository _lotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLotCommandHandler(ILotRepository lotRepository, IUnitOfWork unitOfWork)
    {
        _lotRepository = lotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteLotCommand request, CancellationToken cancellationToken)
    {
        var lot = await _lotRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lot is null)
        {
            return Error.NotFound("Lot.NotFound", "Лот не найден.");
        }

         _lotRepository.Delete(lot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}