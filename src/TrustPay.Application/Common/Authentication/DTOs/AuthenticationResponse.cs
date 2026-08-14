using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Authentication.DTOs
{
    public record AuthenticationResponse(
        Guid Id,
        string NickName,
        string Email,
        string Token,
        string RefreshToken
        );
}
