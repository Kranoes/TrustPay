using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Entities;

namespace TrustPay.Application.Common.Interfaces.Auth
{
    public interface IJwtTokenGenerator
    {
        public string GenerateAccessToken(User user );
        public (string token, DateTime expireAt) GenerateRefreshToken();
        
    }
}
