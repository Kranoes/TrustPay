namespace TrustPay.Application.Lots.Commands.UpdateLot;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

public record UpdateLotCommand(
    Guid Id,
    string Title,
    decimal Amount,
    string Currency,
    int ItemsCount
) : IRequest<Result>;

public class UpdateLotCommandHandler : IRequestHandler<UpdateLotCommand, Result>
{
    private readonly ILotRepository _lotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLotCommandHandler(ILotRepository lotRepository, IUnitOfWork unitOfWork)
    {
        _lotRepository = lotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateLotCommand request, CancellationToken cancellationToken)
    {
        var lot = await _lotRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lot is null)
        {
            return Error.NotFound("Lot.NotFound", "Лот не найден.");
        }

        var moneyResult = Money.Create(request.Amount, request.Currency);
        if (moneyResult.IsFailure)
        {
            return moneyResult.Error;
        }

        var updateDetailsResult = lot.UpdateDetails(request.Title, moneyResult.Value);
        if (updateDetailsResult.IsFailure)
        {
            return updateDetailsResult.Error;
        }

        var updateCountResult = lot.UpdateItemsCount(request.ItemsCount);
        if (updateCountResult.IsFailure)
        {
            return updateCountResult.Error;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}