namespace TrustPay.Domain.Entities;

using System;
using System.Collections.Generic;
using TrustPay.Domain.Common;
using TrustPay.Domain.Events.TagEvents;

public class Tag : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;

    private Tag() { }

    private Tag(Guid id, string name)
        : base(id)
    {
        Name = name;
    }

    public static Result<Tag> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Tag>("Имя тега не может быть пустым.");
        }

        var tag = new Tag(Guid.NewGuid(), name.Trim());
        tag.AddDomainEvent(new TagCreatedDomainEvent(tag.Id, tag.Name));

        return Result.Success(tag);
    }

    public Result UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure("Новое имя тега не может быть пустым.");
        }

        Name = newName.Trim();
        AddDomainEvent(new TagUpdatedDomainEvent(Id, Name));

        return Result.Success();
    }
}