using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Application.Common.Logging;

namespace TrustPay.Application.Common.Authentication.DTOs
{
    public record AuthenticationResponse(
        Guid Id,
        string NickName,
        [property: Sensitive] string Email,
        [property: Sensitive] string Token,
        [property: Sensitive] string RefreshToken
        );
}
