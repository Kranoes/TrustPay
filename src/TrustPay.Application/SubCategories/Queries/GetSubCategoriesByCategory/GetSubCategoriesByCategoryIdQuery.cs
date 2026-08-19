namespace TrustPay.Application.SubCategories.Queries.GetSubCategoriesByCategoryId;

using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.SubCategories.DTOs;
using TrustPay.Domain.Common;

public record GetSubCategoriesByCategoryIdQuery(Guid CategoryId) : IRequest<Result<List<SubCategoryResponse>>>;

public class GetSubCategoriesByCategoryIdQueryHandler : IRequestHandler<GetSubCategoriesByCategoryIdQuery, Result<List<SubCategoryResponse>>>
{
    private readonly ISubCategoryRepository _subCategoryRepository;

    public GetSubCategoriesByCategoryIdQueryHandler(ISubCategoryRepository subCategoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
    }

    public async Task<Result<List<SubCategoryResponse>>> Handle(GetSubCategoriesByCategoryIdQuery request, CancellationToken cancellationToken)
    {
        var subCategories = await _subCategoryRepository.GetByCategoryIdAsync(request.CategoryId, cancellationToken);

        var response = subCategories.Select(sc => new SubCategoryResponse(
            sc.Id,
            sc.CategoryId,
            sc.Title,
            sc.LotsCount,
            sc.TagsIds)).ToList();

        return Result.Success(response);
    }
}