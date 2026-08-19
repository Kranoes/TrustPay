using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Transactions.DTOs
{
    public record WithdrawalResponse(Guid TransactionId, string Status);
}
