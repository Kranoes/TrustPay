using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Domain.Common;
namespace TrustPay.Application.Wallets.Commands.FreezeWallet
{
    public record FreezeWalletCommand(Guid WalletId) : IRequest<Result>;
}
