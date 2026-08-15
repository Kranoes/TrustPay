using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Categories.DTOs;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Categories.Queries.GetAllCategories
{
    public record GetAllCategoriesQuery() : IRequest<Result<List<CategoryResponse>>>;
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryResponse>>>
    {
        private readonly ICategoryRepository _categoryRepository;
        public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<Result<List<CategoryResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            var response = categories.Select(c => new CategoryResponse(
                c.Id,
                c.Title,
                c.Description,
                c.Type
            )).ToList();
            return Result<List<CategoryResponse>>.Success(response);
        }
    }

}
