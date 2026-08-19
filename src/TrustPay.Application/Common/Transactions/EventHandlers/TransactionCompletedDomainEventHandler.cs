using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Models;
using TrustPay.Domain.Events.TransactionsEvents;

namespace TrustPay.Application.Common.Transactions.EventHandlers
{
    public class TransactionCompletedDomainEventHandler 
        :INotificationHandler<DomainEventNotification<TransactionCompletedDomainEvent>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        public TransactionCompletedDomainEventHandler(
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork)
        {
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(
            DomainEventNotification<TransactionCompletedDomainEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;
            if (domainEvent.WalletId is null)
            {
                return;
            }
            var wallet = await _walletRepository.GetByIdAsync(domainEvent.WalletId.Value, cancellationToken);
            if (wallet is null)
            {
                return;
            }
            var depositResult = wallet.Deposit(domainEvent.Amount);
            if (depositResult.IsFailure)
            {
                return;
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
