namespace TrustPay.Application.Categories.Queries.GetCategories;

using MediatR;
using TrustPay.Application.Categories.DTOs;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Models;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;

public record GetCategoriesQuery(
    string[]? Keywords = null,
    Guid? SubCategoryId = null
) : IRequest<Result<List<CategoryResponse>>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryResponse>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<CategoryResponse>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        List<Category> categories;

        if (request.SubCategoryId.HasValue)
        {
            categories = await _categoryRepository.FindBySubCategory(request.SubCategoryId.Value, cancellationToken);
        }
        else if (request.Keywords != null && request.Keywords.Length > 0)
        {
            categories = await _categoryRepository.FindByDescriptionKeywordsAsync(request.Keywords, cancellationToken);
        }
        else
        {
            categories = await _categoryRepository.GetAllAsync(cancellationToken);
        }

        var response = categories.Select(c => new CategoryResponse(
            c.Id,
            c.Title,
            c.Description,
            c.Type
        )).ToList();

        return Result<List<CategoryResponse>>.Success(response);
    }
}