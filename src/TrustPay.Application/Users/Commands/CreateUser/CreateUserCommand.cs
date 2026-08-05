using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Users.Commands.CreateUser
{
    public record class CreateUserCommand (string Email,string NickName) : IRequest<Result<Guid>>;
    
    
}
 