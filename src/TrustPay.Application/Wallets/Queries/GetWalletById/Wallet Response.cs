using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Wallets.Queries.GetWalletById
{
    public record WalletResponse(Guid Id, Guid UserId, decimal Balance, string Currency);
    
}
