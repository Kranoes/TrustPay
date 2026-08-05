using System;
using MediatR;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Lots.Commands.UpdateLot
{
public record UpdateLotCommand(
    Guid LotId,
    Guid UserId,
    string Title,
    decimal Amount,
    string Currency,
    int ItemsCount) : IRequest<Result>;


}