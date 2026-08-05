using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

namespace TrustPay.Application.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Title,
    string Description,
    CategoryType Type
) : IRequest<Result<Guid>>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingCategoryId = await _categoryRepository.GetIdByTitle(request.Title, cancellationToken);
        if (existingCategoryId.HasValue)
        {
            return Result.Failure<Guid>("Категория с таким названием уже существует.");
        }

        var categoryResult = Category.Create(request.Title, request.Description, request.Type);
        if (categoryResult.IsFailure)
        {
            return Result.Failure<Guid>(categoryResult.Error);
        }

        await _categoryRepository.AddAsync(categoryResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(categoryResult.Value.Id);
    }
}