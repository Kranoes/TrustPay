using System;
using System.Collections.Generic;
using System.Linq;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.CategoryEvents;

namespace TrustPay.Domain.Entities
{
    public class Category : AggregateRoot<Guid>
    {
        private readonly List<SubCategory> _subCategories = new();

        public string Title { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public CategoryType Type { get; private set; }

        public IReadOnlyCollection<SubCategory> SubCategories => _subCategories.AsReadOnly();

        private Category() { }

        private Category(Guid id, string title, string description, CategoryType type)
            : base(id)
        {
            Title = title;
            Description = description;
            Type = type;
        }

        public static Result<Category> Create(string title, string description, CategoryType type)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Result.Failure<Category>("Название категории не может быть пустым.");
            }

            var category = new Category(
                Guid.NewGuid(),
                title.Trim(),
                description?.Trim() ?? string.Empty,
                type
            );

            category.AddDomainEvent(new CategoryCreatedDomainEvent(
                category.Id,
                category.Title,
                category.Type));

            return Result.Success(category);
        }

        public Result UpdateDetails(string title, string description, CategoryType type)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Result.Failure("Название категории не может быть пустым.");
            }

            Title = title.Trim();
            Description = description?.Trim() ?? string.Empty;
            Type = type;

            AddDomainEvent(new CategoryUpdatedDomainEvent(Id, Title));

            return Result.Success();
        }

       
        public Result AddSubCategory(SubCategory subCategory)
        {
            if (subCategory == null)
            {
                return Result.Failure("Подкатегория не может быть пустой.");
            }

            if (_subCategories.Any(sc => sc.Id == subCategory.Id))
            {
                return Result.Failure("Эта подкатегория уже привязана к данной категории.");
            }

            _subCategories.Add(subCategory);

            return Result.Success();
        }

       
        public Result RemoveSubCategory(Guid subCategoryId)
        {
            var subCategory = _subCategories.FirstOrDefault(sc => sc.Id == subCategoryId);
            if (subCategory == null)
            {
                return Result.Failure("Подкатегория не привязана к данной категории.");
            }

            _subCategories.Remove(subCategory);

            return Result.Success();
        }

        public Result EnsureCanBeDeleted()
        {
            if (_subCategories.Any())
            {
                return Result.Failure("Нельзя удалить категорию, пока к ней привязаны подкатегории. Сначала отвяжите их.");
            }

            return Result.Success();
        }
    }
}