using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Application.Wallets.Commands.TransferMoney;

public class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository; 

    public TransferMoneyCommandHandler(
        IUnitOfWork unitOfWork,
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository) 
    {
        _unitOfWork = unitOfWork;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result> Handle(TransferMoneyCommand command, CancellationToken cancellationToken)
    {
        var walletSender = await _walletRepository.GetByIdAsync(command.WalletIdSender, cancellationToken);
        if (walletSender is null)
            return Result.Failure($"Кошелек отправителя с ID {command.WalletIdSender} не найден.");

        var walletReceive = await _walletRepository.GetByIdAsync(command.WalletIdReceive, cancellationToken);
        if (walletReceive is null)
            return Result.Failure($"Кошелек получателя с ID {command.WalletIdReceive} не найден.");

        if (walletSender.Id == walletReceive.Id)
        {
            return Result.Failure("Нельзя перевести деньги самому себе.");
        }

        if (walletSender.Status != WalletStatus.Active || walletReceive.Status != WalletStatus.Active)
        {
            return Result.Failure("Один из кошельков неактивен.");
        }

        var debs = Money.Create(command.Amount, command.Currency);
        if (debs.IsFailure)
        {
            return Result.Failure("Ошибка. Неверно указана сумма или валюта.");
        }

        Money money = debs.Value;

        var send = walletSender.Withdraw(money);
        if (send.IsFailure)
        {
            return Result.Failure($"Ошибка списания. Подробнее: {send.Error}");
        }

        var receive = walletReceive.Deposit(money);
        if (receive.IsFailure)
        {
            return Result.Failure($"Ошибка зачисления средств. Подробнее: {receive.Error}");
        }

        var transactionResult = Transaction.CreateTransfer(
        walletSender.Id,
        walletReceive.Id,
        money);

        if (transactionResult.IsFailure)
            return Result.Failure(transactionResult.Error);

        await _transactionRepository.AddAsync(transactionResult.Value, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("Не удалось завершить перевод: баланс был изменен в другом запросе. Повторите попытку.");
        }
    }
}