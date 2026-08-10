using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Enums;

namespace TrustPay.Application.Disputes.DTOs
{
    public record ChangeDisputeStatusRequest(DisputeStatus Status);
}
