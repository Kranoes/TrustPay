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
        private readonly List<Tag> _tags = new();

        public Guid UserId { get; private set; }
        public Guid SubCategoryId { get; private set; }
        public string Title { get; private set; } = null!;
        public Money Cost { get; private set; } = null!;
        public int ItemsCount { get; private set; }

        public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

        public User User { get; private set; } = null!;
        public SubCategory SubCategory { get; private set; } = null!;

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

       
        public Result AddTag(Tag tag)
        {
            if (tag is null)
            {
                return Result.Failure("Тег не может быть null.");
            }

            if (_tags.Any(t => t.Id == tag.Id))
            {
                return Result.Failure("Данный тег уже добавлен к лоту.");
            }

            _tags.Add(tag);

            AddDomainEvent(new LotTagAddedDomainEvent(Id, tag.Id));

            return Result.Success();
        }

        
        public Result RemoveTag(Guid tagId)
        {
            var tagToRemove = _tags.FirstOrDefault(t => t.Id == tagId);
            if (tagToRemove is null)
            {
                return Result.Failure("Тег не найден в списке лота.");
            }

            _tags.Remove(tagToRemove);

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
    }
}