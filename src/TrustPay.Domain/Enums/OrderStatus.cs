using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Domain.Enums
{
    public enum OrderStatus
    {
        Created,
        Paid,
        Delivered,
        Completed,
        Disputed,
        Cancelled
    }
}
