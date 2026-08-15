using System;
using System.Collections.Generic;
using System.Linq;
using TrustPay.Domain.Common;
using TrustPay.Domain.Events.LotEvents;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Domain.Entities
{
    public class Lot : AggregateRoot<Guid>
    {
        private readonly List<Guid> _tagIds = new();

        public Guid UserId { get; private set; }
        public Guid SubCategoryId { get; private set; }
        public string Title { get; private set; } = null!;
        public Money Cost { get; private set; } = null!;
        public int ItemsCount { get; private set; }

        public IReadOnlyCollection<Guid> TagIds => _tagIds.AsReadOnly();

        private Lot() { }

        private Lot(Guid id, Guid userId, Guid subCategoryId, string title, Money cost, int itemsCount)
            : base(id)
        {
            UserId = userId;
            SubCategoryId = subCategoryId;
            Title = title;
            Cost = cost;
            ItemsCount = itemsCount;
        }

        public static Result<Lot> Create(Guid userId, Guid subCategoryId, string title, Money cost, int itemsCount)
        {
            if (userId == Guid.Empty)
            {
                return Result.Failure<Lot>("Идентификатор пользователя не может быть пустым.");
            }

            if (subCategoryId == Guid.Empty)
            {
                return Result.Failure<Lot>("Идентификатор подкатегории не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return Result.Failure<Lot>("Заголовок лота не может быть пустым.");
            }

            if (cost is null)
            {
                return Result.Failure<Lot>("Стоимость лота должна быть указана.");
            }

            if (itemsCount < 0)
            {
                return Result.Failure<Lot>("Количество товаров не может быть отрицательным.");
            }

            var lot = new Lot(
                Guid.NewGuid(),
                userId,
                subCategoryId,
                title.Trim(),
                cost,
                itemsCount);

            lot.AddDomainEvent(new LotCreatedDomainEvent(
                lot.Id,
                lot.UserId,
                lot.SubCategoryId,
                lot.Title,
                lot.Cost,
                lot.ItemsCount));

            return Result.Success(lot);
        }

        public Result UpdateDetails(string newTitle, Money newCost)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                return Result.Failure("Заголовок не может быть пустым.");
            }

            if (newCost is null)
            {
                return Result.Failure("Стоимость должна быть указана.");
            }

            Title = newTitle.Trim();
            Cost = newCost;

            AddDomainEvent(new LotUpdatedDomainEvent(Id, Title, Cost));

            return Result.Success();
        }

        public Result UpdateItemsCount(int newCount)
        {
            if (newCount < 0)
            {
                return Result.Failure("Количество товаров не может быть отрицательным.");
            }

            ItemsCount = newCount;

            AddDomainEvent(new LotItemsCountUpdatedDomainEvent(Id, ItemsCount));

            return Result.Success();
        }

        public Result AddTag(Guid tagId)
        {
            if (tagId == Guid.Empty)
            {
                return Result.Failure("Идентификатор тега не может быть пустым.");
            }

            if (_tagIds.Contains(tagId))
            {
                return Result.Failure("Данный тег уже добавлен к лоту.");
            }

            _tagIds.Add(tagId);

            AddDomainEvent(new LotTagAddedDomainEvent(Id, tagId));

            return Result.Success();
        }

        public Result RemoveTag(Guid tagId)
        {
            if (tagId == Guid.Empty)
            {
                return Result.Failure("Идентификатор тега не может быть пустым.");
            }

            if (!_tagIds.Contains(tagId))
            {
                return Result.Failure("Тег не найден в списке лота.");
            }

            _tagIds.Remove(tagId);

            AddDomainEvent(new LotTagRemovedDomainEvent(Id, tagId));

            return Result.Success();
        }

        public Result ChangeSubCategory(Guid newSubCategoryId)
        {
            if (newSubCategoryId == Guid.Empty)
            {
                return Result.Failure("Идентификатор подкатегории не может быть пустым.");
            }

            SubCategoryId = newSubCategoryId;

            AddDomainEvent(new LotSubCategoryChangedDomainEvent(Id, newSubCategoryId));

            return Result.Success();
        }

        public void LoadTags(IEnumerable<Guid> tagIds)
        {
            _tagIds.Clear();

            if (tagIds is null)
            {
                return;
            }

            foreach (var tagId in tagIds)
            {
                if (tagId != Guid.Empty && !_tagIds.Contains(tagId))
                {
                    _tagIds.Add(tagId);
                }
            }
        }
    }
}