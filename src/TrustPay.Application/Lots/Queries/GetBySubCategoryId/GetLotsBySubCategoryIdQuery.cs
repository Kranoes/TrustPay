namespace TrustPay.Application.Lots.Queries.GetLotsBySubCategoryId;

using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Lots.DTOs;
using TrustPay.Domain.Common;

public record GetLotsBySubCategoryIdQuery(Guid SubCategoryId) : IRequest<Result<List<LotResponse>>>;

public class GetLotsBySubCategoryIdQueryHandler : IRequestHandler<GetLotsBySubCategoryIdQuery, Result<List<LotResponse>>>
{
    private readonly ILotRepository _lotRepository;

    public GetLotsBySubCategoryIdQueryHandler(ILotRepository lotRepository)
    {
        _lotRepository = lotRepository;
    }

    public async Task<Result<List<LotResponse>>> Handle(GetLotsBySubCategoryIdQuery request, CancellationToken cancellationToken)
    {
        var lots = await _lotRepository.GetBySubCategoryIdAsync(request.SubCategoryId, cancellationToken);

        var response = lots.Select(lot => new LotResponse(
            lot.Id,
            lot.UserId,
            lot.SubCategoryId,
            lot.Title,
            lot.Cost.Amount,
            lot.Cost.Currency,
            lot.ItemsCount
        )).ToList();

        return Result<List<LotResponse>>.Success(response);
    }
}