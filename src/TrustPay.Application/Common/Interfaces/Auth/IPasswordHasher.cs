using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Interfaces.Auth
{
    public interface IPasswordHasher
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string passwordHash, string passwordBase);
    }
}
