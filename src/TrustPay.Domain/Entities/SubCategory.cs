using System;
using System.Collections.Generic;
using System.Linq;
using TrustPay.Domain.Common;
using TrustPay.Domain.Events.SubCategoryEvents;

namespace TrustPay.Domain.Entities
{
    public class SubCategory : AggregateRoot<Guid>
    {
        private readonly List<Tag> _tags = new();

        public Guid CategoryId { get; private set; }
        public string Title { get; private set; } = null!;
        public int LotsCount { get; private set; }

        public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

        public Category Category { get; private set; } = null!;

        private SubCategory() { }

        private SubCategory(Guid id, Guid categoryId, string title)
            : base(id)
        {
            CategoryId = categoryId;
            Title = title;
            LotsCount = 0;
        }

        public static Result<SubCategory> Create(Guid categoryId, string title)
        {
            if (categoryId == Guid.Empty)
            {
                return Result.Failure<SubCategory>("Идентификатор категории не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return Result.Failure<SubCategory>("Заголовок подкатегории не может быть пустым.");
            }

            var subCategory = new SubCategory(
                Guid.NewGuid(),
                categoryId,
                title.Trim());

            subCategory.AddDomainEvent(new SubCategoryCreatedDomainEvent(
                subCategory.Id,
                subCategory.CategoryId,
                subCategory.Title));

            return Result.Success(subCategory);
        }

        public Result UpdateTitle(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                return Result.Failure("Заголовок подкатегории не может быть пустым.");
            }

            Title = newTitle.Trim();

            AddDomainEvent(new SubCategoryTitleUpdatedDomainEvent(Id, Title));

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
                return Result.Failure("Данный тег уже добавлен к подкатегории.");
            }

            _tags.Add(tag);

            AddDomainEvent(new SubCategoryTagAddedDomainEvent(Id, tag.Id));

            return Result.Success();
        }

        public Result RemoveTag(Guid tagId)
        {
            var tagToRemove = _tags.FirstOrDefault(t => t.Id == tagId);
            if (tagToRemove is null)
            {
                return Result.Failure("Тег не найден в подкатегории.");
            }

            _tags.Remove(tagToRemove);

            AddDomainEvent(new SubCategoryTagRemovedDomainEvent(Id, tagId));

            return Result.Success();
        }

        public void IncrementLotsCount()
        {
            LotsCount++;
        }

        public void DecrementLotsCount()
        {
            if (LotsCount > 0)
            {
                LotsCount--;
            }
        }
    }
}