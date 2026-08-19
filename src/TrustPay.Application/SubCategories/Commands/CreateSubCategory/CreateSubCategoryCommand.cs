namespace TrustPay.Application.SubCategories.Commands.CreateSubCategory;

using FluentValidation;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;

public record CreateSubCategoryCommand(Guid CategoryId, string Title) : IRequest<Result<Guid>>;



public class CreateSubCategoryCommandHandler : IRequestHandler<CreateSubCategoryCommand, Result<Guid>>
{
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubCategoryCommandHandler(
        ISubCategoryRepository subCategoryRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _subCategoryRepository = subCategoryRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<Guid>("Родительская категория не найдена.");
        }

        var subCategoryResult = SubCategory.Create(request.CategoryId, request.Title);
        if (subCategoryResult.IsFailure)
        {
            return Result.Failure<Guid>(subCategoryResult.Error);
        }

        await _subCategoryRepository.AddAsync(subCategoryResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(subCategoryResult.Value.Id);
    }
}