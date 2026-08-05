using System;
using System.Collections.Generic;
using TrustPay.Domain.Common;
using TrustPay.Domain.Events.TagEvents;

namespace TrustPay.Domain.Entities
{
    public class Tag : AggregateRoot<Guid>
    {
        private readonly List<Lot> _lots = new();
        private readonly List<SubCategory> _subCategories = new();

        public string Name { get; private set; } = null!;
        public string NormalizedName { get; private set; } = null!;

        public IReadOnlyCollection<Lot> Lots => _lots.AsReadOnly();
        public IReadOnlyCollection<SubCategory> SubCategories => _subCategories.AsReadOnly();

        private Tag() { }

        private Tag(Guid id, string name, string normalizedName)
            : base(id)
        {
            Name = name;
            NormalizedName = normalizedName;
        }

        public static Result<Tag> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<Tag>("Имя тега не может быть пустым.");
            }

            string trimmedName = name.Trim();

            var tag = new Tag(
                Guid.NewGuid(),
                trimmedName,
                trimmedName.ToUpperInvariant());

            tag.AddDomainEvent(new TagCreatedDomainEvent(tag.Id, tag.Name));

            return Result.Success(tag);
        }

        public Result UpdateName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                return Result.Failure("Новое имя тега не может быть пустым.");
            }

            string trimmedName = newName.Trim();
            Name = trimmedName;
            NormalizedName = trimmedName.ToUpperInvariant();

            AddDomainEvent(new TagUpdatedDomainEvent(Id, Name));

            return Result.Success();
        }
    }
}