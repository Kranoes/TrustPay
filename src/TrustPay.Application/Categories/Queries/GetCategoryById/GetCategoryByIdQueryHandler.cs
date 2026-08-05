using MediatR;
using TrustPay.Application.Categories.DTOs;
using TrustPay.Application.Categories.Queries.GetCategoryById;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    public async Task<Result<CategoryResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.id,cancellationToken);
        if (category == null)
        {
            return Result<CategoryResponse>.Failure("Category not found");
        }
        var response = new CategoryResponse(category.Id, category.Title, category.Description, category.Type);

        return Result<CategoryResponse>.Success(response);
    }
}