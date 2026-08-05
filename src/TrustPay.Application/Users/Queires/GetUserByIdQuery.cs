using System;
using System.Collections.Generic;
using System.Text;
using MediatR;


using TrustPay.Domain.Common;
namespace TrustPay.Application.Users.Queires
{
    public record GetUserByIdQuery (Guid Id) : IRequest<Result<UserResponse>>; 
    
    
}
