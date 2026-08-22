namespace TrustPay.Domain.Entities;

using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.OrderEvents;
using TrustPay.Domain.ValueObjects;

public class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public Guid ExecutorId { get; private set; }
    public Guid LotId { get; private set; }
    public int Quantity { get; private set; }
    public Money Price { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public uint Version { get; private set; }

    private Order() { }

    private Order(
        Guid id,
        Guid customerId,
        Guid executorId,
        Guid lotId,
        int quantity,
        Money price)
        : base(id)
    {
        CustomerId = customerId;
        ExecutorId = executorId;
        LotId = lotId;
        Quantity = quantity;
        Price = price;
        Status = OrderStatus.Created;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Order> Create(
        Guid customerId,
        Guid executorId,
        Guid lotId,
        int quantity,
        Money price)
    {
        if (customerId == Guid.Empty)
        {
            return Result.Failure<Order>("Идентификатор покупателя не может быть пустым.");
        }

        if (executorId == Guid.Empty)
        {
            return Result.Failure<Order>("Идентификатор исполнителя не может быть пустым.");
        }

        if (customerId == executorId)
        {
            return Result.Failure<Order>("Заказчик и исполнитель не могут быть одним лицом.");
        }

        if (lotId == Guid.Empty)
        {
            return Result.Failure<Order>("Идентификатор лота не может быть пустым.");
        }

        if (quantity <= 0)
        {
            return Result.Failure<Order>("Количество товара должно быть больше нуля.");
        }

        if (price is null)
        {
            return Result.Failure<Order>("Цена должна быть указана.");
        }

        var order = new Order(
            Guid.NewGuid(),
            customerId,
            executorId,
            lotId,
            quantity,
            price);

        order.AddDomainEvent(new OrderCreatedDomainEvent(
            order.Id,
            order.CustomerId,
            order.ExecutorId,
            order.LotId,
            order.Quantity,
            order.Price));

        return Result.Success(order);
    }

    public Result StartProcess(Guid requestingUserId)
    {
        if (requestingUserId != ExecutorId)
        {
            return Result.Failure("Только исполнитель может приступить к выполнению заказа.");
        }

        if (Status != OrderStatus.Created)
        {
            return Result.Failure("Приступить к работе можно только для заказа в статусе Created.");
        }

        Status = OrderStatus.InProgress;
        AddDomainEvent(new OrderStartedDomainEvent(Id));

        return Result.Success();
    }

    public Result Complete(Guid requestingUserId)
    {
        if (requestingUserId != CustomerId)
        {
            return Result.Failure("Только заказчик может подтвердить успешное завершение заказа.");
        }

        if (Status != OrderStatus.InProgress)
        {
            return Result.Failure("Завершить можно только заказ, находящийся в работе.");
        }

        Status = OrderStatus.Completed;
        AddDomainEvent(new OrderCompletedDomainEvent(Id, CustomerId, ExecutorId));

        return Result.Success();
    }

    public Result AttachDispute(Guid disputeId)
    {
        if (disputeId == Guid.Empty)
        {
            return Result.Failure("Идентификатор спора не может быть пустым.");
        }

        if (Status == OrderStatus.Disputed)
        {
            return Result.Failure("Для данного заказа спор уже открыт.");
        }

        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
        {
            return Result.Failure("Нельзя открыть спор по завершенному или отмененному заказу.");
        }

        Status = OrderStatus.Disputed;
        AddDomainEvent(new OrderDisputedDomainEvent(Id, disputeId));

        return Result.Success();
    }

    public Result Cancel(Guid requestingUserId, bool isAdmin)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
        {
            return Result.Failure("Завершенный или уже отмененный заказ нельзя отменить.");
        }

        var isCustomer = requestingUserId == CustomerId;

        if (Status == OrderStatus.InProgress && isCustomer && !isAdmin)
        {
            return Result.Failure("Заказ уже в работе. Отменить его может только исполнитель или администратор через спор.");
        }

        Status = OrderStatus.Cancelled;
        AddDomainEvent(new OrderCancelledDomainEvent(Id, requestingUserId));

        return Result.Success();
    }
}