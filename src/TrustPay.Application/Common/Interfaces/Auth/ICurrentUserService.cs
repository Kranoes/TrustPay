using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Interfaces.Auth
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
    }
}
