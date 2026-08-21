using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Models;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Events.UserEvents;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Application.Users.EventHandlers
{
    public sealed class UserCreatedDomainEventHandler : INotificationHandler<DomainEventNotification<UserCreatedDomainEvent>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger <UserCreatedDomainEventHandler> _logger;
        public UserCreatedDomainEventHandler(
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork,
            ILogger<UserCreatedDomainEventHandler> logger
            )
        {
            _unitOfWork = unitOfWork;
            _walletRepository = walletRepository;
            _logger = logger;
        }
        public async Task Handle(
            DomainEventNotification<UserCreatedDomainEvent> notification,
            CancellationToken cancellationToken)
        {
            var userEvent = notification.DomainEvent;
            var amount = Money.Zero("RUB");
            var walletResult = Wallet.Create(userEvent.UserId, amount);
            if (walletResult.IsFailure)
            {
                _logger.LogError("Failed to create wallet for user {UserId}. Error: {Error}",
                userEvent.UserId,
                walletResult.Error);
                return;
            }
            await _walletRepository.AddAsync(walletResult.Value, cancellationToken);
            _logger.LogInformation(
                "Wallet {WalletId} created for user {UserId}",
            walletResult.Value.Id,
            userEvent.UserId);
        }


    }
}

