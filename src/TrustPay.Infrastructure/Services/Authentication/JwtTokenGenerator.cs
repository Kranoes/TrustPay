using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Services.Authentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IOptions<JwtSettings> _jwtOptions;
        
        public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
        {
            _jwtOptions = jwtOptions;

        }
        public string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.Secret));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            Claim [] claims = 
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(ClaimTypes.Role,user.Role.ToString())
            };
            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Value.Issuer,
                audience: _jwtOptions.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Value.AccessTokenExpirationMinutes),
                signingCredentials: signingCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);

        }
        public (string token,string tokenHash, DateTime expireAt) GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            RandomNumberGenerator.Fill(randomBytes);
            var token = Convert.ToBase64String(randomBytes);
            var tokenHash = GetRefreshTokenHash(token);
            var expireAt = DateTime.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenExpiryDays);
            return (token, tokenHash, expireAt);
        }
        public string GetRefreshTokenHash(string token)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }
    }
}
