using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Transactions.DTOs
{
    public record TransactionResponse(Guid Id,
        Guid? SenderWalletId,
        Guid? ReceiverWalletId,
        decimal Amount,
        string Currency,
        string Type,
        string Status,
        DateTime CreatedAt);
    
}
