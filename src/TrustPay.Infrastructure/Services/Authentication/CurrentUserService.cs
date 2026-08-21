using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using TrustPay.Application.Common.Interfaces.Auth;

namespace TrustPay.Infrastructure.Services.Authentication
{
    public class CurrentUserService :ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid UserId
        {
            get 
            {
                var idClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(idClaim, out var userId) ? userId : Guid.Empty;
            }
        }
        public bool IsAdmin => IsInRole("Admin");
        public bool IsArbitrator => IsInRole("Arbitrator");
        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
        }
    }
}
