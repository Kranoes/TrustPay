namespace TrustPay.Application.SubCategories.Commands.UpdateSubCategoryTitle;

using FluentValidation;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record UpdateSubCategoryTitleCommand(Guid Id, string NewTitle) : IRequest<Result>;


public class UpdateSubCategoryTitleCommandHandler : IRequestHandler<UpdateSubCategoryTitleCommand, Result>
{
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSubCategoryTitleCommandHandler(
        ISubCategoryRepository subCategoryRepository,
        IUnitOfWork unitOfWork)
    {
        _subCategoryRepository = subCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateSubCategoryTitleCommand request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (subCategory is null)
        {
            return Result.Failure("Подкатегория не найдена.");
        }

        var updateResult = subCategory.UpdateTitle(request.NewTitle);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        _subCategoryRepository.Update(subCategory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}