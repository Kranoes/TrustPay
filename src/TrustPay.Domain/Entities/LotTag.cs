using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Domain.Entities
{
    public class LotTag
    {
        public Guid LotId { get; set; }
        public Guid TagId { get; set; }
    }
}
