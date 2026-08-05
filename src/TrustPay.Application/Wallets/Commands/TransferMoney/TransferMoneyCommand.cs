using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Application.Wallets.Commands.TransferMoney
{
    public record TransferMoneyCommand(Guid WalletIdSender,Guid WalletIdReceive,decimal Amount,string Currency) : IRequest<Result>;
    
}
