using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Enums;

namespace TrustPay.Application.Users.DTO
{
    public record UserResponse(Guid Id, string Email, string NickName,UserRole Role);
    
}
