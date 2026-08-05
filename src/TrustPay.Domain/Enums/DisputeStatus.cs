using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Domain.Enums
{
    public enum DisputeStatus
    {
        Opened,
        UnderReview,
        ResolvedForBuyer,
        ResolvedForSeller,
        Closed

    }
}
