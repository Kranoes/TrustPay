using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Application.Wallets.Commands.DepositMoney
{
    public record DepositMoneyCommand(Guid WalletId,decimal Amount,string Currency = "RUB") : IRequest<Result>;
    
    
}
