using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Users.Queires
{
    public record UserResponse(Guid Id, Guid? WalletId, string Email, string NickName);
    
}
