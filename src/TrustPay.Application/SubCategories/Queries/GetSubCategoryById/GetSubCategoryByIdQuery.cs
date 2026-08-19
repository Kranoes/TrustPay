namespace TrustPay.Application.SubCategories.Queries.GetSubCategoryById;

using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.SubCategories.DTOs;
using TrustPay.Domain.Common;

public record GetSubCategoryByIdQuery(Guid Id) : IRequest<Result<SubCategoryResponse>>;

public class GetSubCategoryByIdQueryHandler : IRequestHandler<GetSubCategoryByIdQuery, Result<SubCategoryResponse>>
{
    private readonly ISubCategoryRepository _subCategoryRepository;

    public GetSubCategoryByIdQueryHandler(ISubCategoryRepository subCategoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
    }

    public async Task<Result<SubCategoryResponse>> Handle(GetSubCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (subCategory is null)
        {
            return Result.Failure<SubCategoryResponse>("Подкатегория не найдена.");
        }

        var response = new SubCategoryResponse(
            subCategory.Id,
            subCategory.CategoryId,
            subCategory.Title,
            subCategory.LotsCount,
            subCategory.TagsIds);

        return Result.Success(response);
    }
}