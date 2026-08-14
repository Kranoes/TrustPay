using System;
using System.Collections.Generic;
using System.Text;
using Isopoh.Cryptography.Argon2;
using TrustPay.Application.Common.Interfaces.Auth;

namespace TrustPay.Infrastructure.Services.Authentication
{
    public class PasswordHasher :IPasswordHasher
    {
        public  string HashPassword(string password)
        {
            return  Argon2.Hash(password);
        }
        public bool VerifyPassword(string passwordHash, string password)
        {
            return Argon2.Verify(passwordHash, password);
        }

    }
}
