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
        public string Title { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public CategoryType Type { get; private set; }


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

        public Result UpdateDetails(string? title, string? description, CategoryType? type)
        {
            if (title is not null)
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return Result.Failure("Название категории не может быть пустым.");
                }
                Title = title.Trim();
            }
            if (description is not null)
            {
                Description = description.Trim();
            }
            if (type.HasValue)
            {
                Type = type.Value;
            }

            AddDomainEvent(new CategoryUpdatedDomainEvent(Id, Title));

            return Result.Success();
        }

       
        
    }
}