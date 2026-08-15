using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Categories.DTOs;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

namespace TrustPay.Application.Categories.Queries.SearchCategories;

    public record SearchCategoriesQuery(
        string? Title = null,
        string? Description = null,
        CategoryType? Type = null 
        ) : IRequest<Result<List<CategoryResponse>>>;
public class SearchCategoriesQueryHandler : IRequestHandler<SearchCategoriesQuery, Result<List<CategoryResponse>>>
{
    private readonly ICategoryRepository _categoryRepository;
    public SearchCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    public async Task<Result<List<CategoryResponse>>> Handle(SearchCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.SearchAsync(request.Title, request.Description, request.Type, cancellationToken);
        var categoryResponses = categories.Select(c => new CategoryResponse(c.Id, c.Title, c.Description, c.Type)).ToList();
        return Result<List<CategoryResponse>>.Success(categoryResponses);
    }
}
