using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

namespace TrustPay.Application.Wallets.Queries.GetWalletById;

public record GetWalletByIdQuery(Guid WalletId) : IRequest<Result<WalletResponse>>;

public class GetWalletByIdQueryHandler : IRequestHandler<GetWalletByIdQuery, Result<WalletResponse>>
    {
        private readonly IWalletRepository _walletRepository;
        public GetWalletByIdQueryHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }
        public async Task<Result<WalletResponse>> Handle(GetWalletByIdQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId,cancellationToken);
            if (wallet == null)
            {
                return Result.Failure<WalletResponse>($"Кошелек с ID {request.WalletId} не найден.");

            }

            var response = new WalletResponse(wallet.Id, wallet.UserId, wallet.AvailableBalance.Amount, wallet.AvailableBalance.Currency);
            return Result.Success(response);
        }
    }

