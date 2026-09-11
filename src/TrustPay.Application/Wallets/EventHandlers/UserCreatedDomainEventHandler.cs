namespace TrustPay.Application.Wallets.EventHandlers;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Models;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Events.UserEvents;
using TrustPay.Domain.ValueObjects;

public class UserCreatedDomainEventHandler : INotificationHandler<DomainEventNotification<UserCreatedDomainEvent>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserCreatedDomainEventHandler(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DomainEventNotification<UserCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var existingWallet = await _walletRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        if (existingWallet is not null)
        {
            return;
        }

        var moneyResult = Money.Create(0, "RUB");
        if (moneyResult.IsFailure)
        {
            throw new InvalidOperationException($"Ошибка создания денег для кошелька: {moneyResult.Error}");
        }

        var walletResult = Wallet.Create(domainEvent.UserId, moneyResult.Value);
        if (walletResult.IsFailure)
        {
            throw new InvalidOperationException($"Ошибка создания кошелька: {walletResult.Error}");
        }

        await _walletRepository.AddAsync(walletResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}