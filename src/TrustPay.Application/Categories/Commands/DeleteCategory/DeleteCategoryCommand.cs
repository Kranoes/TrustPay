using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubCategoryRepository _subCategoryRepository;
    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, ISubCategoryRepository subCategoryRepository)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _subCategoryRepository = subCategoryRepository;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            return Result.Failure("Категория не найдена.");
        }

        var canDeleteResult = await _subCategoryRepository.HasByCategoryIdAsync(request.Id, cancellationToken);
        if (canDeleteResult)
        {
            return Result.Failure("Невозможно удалить категорию, так как она имеет связанные подкатегории.");
        }

        _categoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}