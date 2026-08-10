using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Disputes.DTO
{
    public record CreateDisputeRequest(
    Guid OrderId,
    Guid CustomerId,
    Guid ExecutorId,
    string Reason);
}
