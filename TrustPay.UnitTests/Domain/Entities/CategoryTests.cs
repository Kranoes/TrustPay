using FluentAssertions;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.CategoryEvents;

namespace TrustPay.UnitTests.Domain.Entities
{
    public class CategoryTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var title = "Test Category";
            var description = "This is a test category.";
            var type = CategoryType.Other;
            var category = Category.Create(title, description, type);
            category.IsSuccess.Should().BeTrue();
            category.Value.Title.Should().Be(title);
            category.Value.Description.Should().Be(description);
            category.Value.Type.Should().Be(type);
        }
        [Fact]
        public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
        {
            var title = "";
            var description = "This is a test category.";
            var type = CategoryType.Other;
            var category = Category.Create(title, description, type);
            category.IsFailure.Should().BeTrue();
            category.Error.Description.Should().Be("Название категории не может быть пустым.");
        }
        [Fact]
        public void Update_ShouldReturnSuccess_WhenDataIsValid()
        {
            var title = "Test Category";
            var description = "This is a test category.";
            var type = CategoryType.Other;
            var newTitle = "Updated Category";
            var newDescription = "This is an updated test category.";
            var newType = CategoryType.Service;
            var category = Category.Create(title, description, type);
            var result = category.Value.UpdateDetails(newTitle, newDescription, newType);
            result.IsSuccess.Should().BeTrue();
            result.Value.Title.Should().Be(newTitle);
            result.Value.Description.Should().Be(newDescription);
            result.Value.Type.Should().Be(newType);
            

        }
        [Fact]
        public void Update_ShouldReturnFailure_WhenTitleIsEmpty()
        {
            var title = "Test Category";
            var description = "This is a test category.";
            var type = CategoryType.Other;
            var newTitle = "";
            var newDescription = "This is an updated test category.";
            var newType = CategoryType.Service;
            var category = Category.Create(title, description, type);
            var result = category.Value.UpdateDetails(newTitle, newDescription, newType);
            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Название категории не может быть пустым.");
        }
        [Fact]
        public void Update_ShouldGenerateNewDomainEvent_WhenDataIsValid()
        {
            var title = "Test Category";
            var description = "This is a test category.";
            var type = CategoryType.Other;
            var newTitle = "Updated Category";
            var newDescription = "This is an updated test category.";
            var newType = CategoryType.Service;
            var category = Category.Create(title, description, type);
            category.Value.UpdateDetails(newTitle, newDescription, newType);
            category.Value.DomainEvents.Should().ContainSingle(e => e is CategoryUpdatedDomainEvent);
        }

    }
}
