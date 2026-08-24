namespace TrustPay.Application.SubCategories.Commands.DeleteSubCategory;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record DeleteSubCategoryCommand(Guid Id) : IRequest<Result>;

public class DeleteSubCategoryCommandHandler : IRequestHandler<DeleteSubCategoryCommand, Result>
{
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSubCategoryCommandHandler(
        ISubCategoryRepository subCategoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _subCategoryRepository = subCategoryRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteSubCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            return Result.Failure("Недостаточно прав для выполнения операции.");
        }

        var subCategory = await _subCategoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (subCategory is null)
        {
            return Result.Failure("Подкатегория не найдена.");
        }

        if (subCategory.LotsCount > 0)
        {
            return Result.Failure("Нельзя удалить подкатегорию, содержащую активные лоты.");
        }

        _subCategoryRepository.Delete(subCategory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}