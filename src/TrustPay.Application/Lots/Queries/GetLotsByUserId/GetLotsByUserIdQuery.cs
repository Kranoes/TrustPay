namespace TrustPay.Application.Lots.Queries.GetLotsByUserId;

using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Lots.DTOs;
using TrustPay.Domain.Common;

public record GetLotsByUserIdQuery(Guid UserId) : IRequest<Result<List<LotResponse>>>;

public class GetLotsByUserIdQueryHandler : IRequestHandler<GetLotsByUserIdQuery, Result<List<LotResponse>>>
{
    private readonly ILotRepository _lotRepository;

    public GetLotsByUserIdQueryHandler(ILotRepository lotRepository)
    {
        _lotRepository = lotRepository;
    }

    public async Task<Result<List<LotResponse>>> Handle(GetLotsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var lots = await _lotRepository.GetByUserIdAsync(request.UserId, cancellationToken);

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