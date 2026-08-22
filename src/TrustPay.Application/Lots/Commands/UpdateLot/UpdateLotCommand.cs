namespace TrustPay.Application.Lots.Commands.UpdateLot;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

public record UpdateLotCommand(
    Guid Id,
    string? Title,
    decimal? Amount,
    string? Currency,
    int? ItemsCount
) : IRequest<Result>;

public class UpdateLotCommandHandler : IRequestHandler<UpdateLotCommand, Result>
{
    private readonly ILotRepository _lotRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLotCommandHandler(
        ILotRepository lotRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _lotRepository = lotRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateLotCommand request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserService.UserId;
        var lot = await _lotRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lot is null)
        {
            return Error.NotFound("Lot.NotFound", "Лот не найден.");
        }

        if (lot.UserId != currentUser && !_currentUserService.IsAdmin)
        {
            return Result.Failure("Только создатель лота или админ может изменить лот.");
        }

        Money newPrice = lot.Cost;
        if (request.Amount.HasValue || !string.IsNullOrWhiteSpace(request.Currency))
        {
            var amount = request.Amount ?? lot.Cost.Amount;
            var currency = request.Currency ?? lot.Cost.Currency;

            var moneyResult = Money.Create(amount, currency);
            if (moneyResult.IsFailure)
            {
                return moneyResult.Error;
            }

            newPrice = moneyResult.Value;
        }

        var titleToUpdate = request.Title ?? lot.Title;
        var updateDetailsResult = lot.UpdateDetails(titleToUpdate, newPrice);
        if (updateDetailsResult.IsFailure)
        {
            return updateDetailsResult.Error;
        }

        if (request.ItemsCount.HasValue)
        {
            var updateCountResult = lot.UpdateItemsCount(request.ItemsCount.Value);
            if (updateCountResult.IsFailure)
            {
                return updateCountResult.Error;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}