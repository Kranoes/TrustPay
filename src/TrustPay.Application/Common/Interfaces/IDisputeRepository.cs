namespace TrustPay.Application.Common.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

public interface IDisputeRepository
{
    Task<Dispute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получение списка споров с возможностью фильтрации по статусу, участникам и ключевым словам
    /// </summary>
    Task<List<Dispute>> GetFilteredAsync(
        DisputeStatus? status,
        Guid? customerId,
        Guid? executorId,
        Guid? arbitratorId,
        string[]? keywords,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetCustomerIdByDisputeAsync(Guid disputeId, CancellationToken cancellationToken = default);
    Task<Guid?> GetExecutorIdByDisputeAsync(Guid disputeId, CancellationToken cancellationToken = default);
    Task<DisputeStatus?> GetStatusOfDisputeByIdAsync(Guid disputeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Находит ID наиболее свободного арбитра/админа для авто-назначения
    /// </summary>
    Task<Guid?> GetAvailableArbitratorIdAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Dispute dispute, CancellationToken cancellationToken = default);
    void Update(Dispute dispute);
}