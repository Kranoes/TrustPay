namespace TrustPay.Application.Lots.Commands.CreateLot;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;

public record CreateLotCommand(
    Guid UserId,
    Guid SubCategoryId,
    string Title,
    decimal Amount,
    string Currency,
    int ItemsCount
) : IRequest<Result<Guid>>;

public class CreateLotCommandHandler : IRequestHandler<CreateLotCommand, Result<Guid>>
{
    private readonly ILotRepository _lotRepository;
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLotCommandHandler(
        ILotRepository lotRepository,
        ISubCategoryRepository subCategoryRepository,
        IUnitOfWork unitOfWork)
    {
        _lotRepository = lotRepository;
        _subCategoryRepository = subCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateLotCommand request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategoryRepository.GetByIdAsync(request.SubCategoryId, cancellationToken);
        if (subCategory is null)
        {
            return Error.NotFound("SubCategory.NotFound", "Указанная подкатегория не найдена.");
        }

        var moneyResult = Money.Create(request.Amount, request.Currency);
        if (moneyResult.IsFailure)
        {
            return moneyResult.Error; 
        }

        var lotResult = Lot.Create(
            request.UserId,
            request.SubCategoryId,
            request.Title,
            moneyResult.Value,
            request.ItemsCount);

        if (lotResult.IsFailure)
        {
            return lotResult.Error; 
        }

        var lot = lotResult.Value;

        await _lotRepository.AddAsync(lot, cancellationToken);
        subCategory.IncrementLotsCount();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lot.Id;
    }
}