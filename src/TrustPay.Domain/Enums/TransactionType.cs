using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Domain.Enums
{
    public enum TransactionType
    {
        Transfer = 1,
        Deposit = 2,
        Withdrawal = 3
    }
    public enum TransactionStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3 
    }
}
