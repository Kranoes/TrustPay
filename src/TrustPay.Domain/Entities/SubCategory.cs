using System;
using System.Collections.Generic;
using System.Linq;
using TrustPay.Domain.Common;
using TrustPay.Domain.Events.SubCategoryEvents;

namespace TrustPay.Domain.Entities
{
    public class SubCategory : AggregateRoot<Guid>
    {
        public Guid CategoryId { get; private set; }
        public string Title { get; private set; } = null!;
        public int LotsCount { get; private set; }
        private readonly HashSet<Guid> _tagsIds = new();
        public IReadOnlyCollection<Guid> TagsIds => _tagsIds;


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

        public Result AddTag(Guid tagId)
        {
            if (tagId == Guid.Empty)
            {
                return Result.Failure("Тег не может быть пустым.");
            }

            if (_tagsIds.Add(tagId))
            {
                AddDomainEvent(new SubCategoryTagAddedDomainEvent(Id, tagId));
                return Result.Success();


            }
            return Result.Failure("Данный тег уже добавлен к подкатегории.");
        }

        public Result RemoveTag(Guid tagId)
        {
            if(tagId == Guid.Empty)
            {
                return Result.Failure("Идентификатор тега не может быть пустым.");
            }
            if(_tagsIds.Remove(tagId))
            {
                AddDomainEvent(new SubCategoryTagRemovedDomainEvent(Id, tagId));
                return Result.Success();


            }
            return Result.Failure("Тег не найден в подкатегории.");

        }
        public void LoatTags(IEnumerable<Guid> tagIds)
        {
            _tagsIds.Clear();
            if (tagIds is null)
            {
                return;
            }
            foreach (var tagId in tagIds)
            {
                if (tagId != Guid.Empty)
                {
                    _tagsIds.Add(tagId);
                }
            }
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