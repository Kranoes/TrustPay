using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Wallets.Queries.GetWalletById
{
    public record GetWalletByIdQuery(Guid WalletId) : IRequest<Result<WalletResponse>>;
    
}
