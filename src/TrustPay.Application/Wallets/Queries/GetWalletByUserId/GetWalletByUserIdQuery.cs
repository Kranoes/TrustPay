using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Wallets.Queries.GetWalletById;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Wallets.Queries.GetWalletByUserId
{
    public record GetWalletByUserIdQuery(Guid UserId) : IRequest<Result<WalletResponse>>;
    public class GetWalletByUserIdQueryHandler : IRequestHandler<GetWalletByUserIdQuery, Result<WalletResponse>>
    {
        private readonly IWalletRepository _walletRepository;

        public GetWalletByUserIdQueryHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<Result<WalletResponse>> Handle(
            GetWalletByUserIdQuery request,
            CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            if (wallet is null)
            {
                return Error.NotFound("Wallet.NotFound", "Кошелек пользователя не найден.");
            }

            return new WalletResponse(
                wallet.Id,
                wallet.UserId,
                wallet.AvailableBalance.Amount,
                wallet.AvailableBalance.Currency);
        }
    }
}
