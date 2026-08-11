using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Domain.Common;
using TrustPay.Application.Lots.DTOs;
using TrustPay.Application.Common.Interfaces;
namespace TrustPay.Application.Lots.Queries.GetById
{
    public record GetByIdQuery(Guid Id) : IRequest<Result<LotResponse>>;
    public class GetByIdQueryHandler : IRequestHandler<GetByIdQuery, Result<LotResponse>>
    {
        private readonly ILotRepository _lotRepository;
        public GetByIdQueryHandler(ILotRepository lotRepository, IUnitOfWork unitOfWork)
        {
            _lotRepository = lotRepository;
        }
        public async Task<Result<LotResponse>> Handle(GetByIdQuery request, CancellationToken cancellationToken) 
        {
        
        var response = await _lotRepository.GetByIdAsync(request.Id, cancellationToken);
            if (response is null)
            {
                return Error.NotFound("Lot.NotFound", "Lot not found");
            }
            LotResponse lotResponse = new LotResponse
            (
                response.Id,
                response.UserId,
                response.SubCategoryId,
                response.Title,
                response.Cost.Amount,
                response.Cost.Currency,
                response.ItemsCount
            );
            return Result<LotResponse>.Success(lotResponse);
        }
    }
}
