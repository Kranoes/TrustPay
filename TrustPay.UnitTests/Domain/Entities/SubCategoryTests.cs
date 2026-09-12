using FluentAssertions;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Events.SubCategoryEvents;

namespace TrustPay.UnitTests.Domain.Entities
{
    public class SubCategoryTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var title = "Test SubCategory";
            var categoryId = Guid.NewGuid();

            var subCategory = SubCategory.Create(categoryId, title);

            subCategory.IsSuccess.Should().BeTrue();
            subCategory.Value.Title.Should().Be(title);
            subCategory.Value.CategoryId.Should().Be(categoryId);
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenCategoryIdIsEmpty()
        {
            var subCategory = SubCategory.Create(Guid.Empty, "Test SubCategory");

            subCategory.IsFailure.Should().BeTrue();
            subCategory.Error.Description.Should().Be("Идентификатор категории не может быть пустым.");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
        {
            var subCategory = SubCategory.Create(Guid.NewGuid(), string.Empty);

            subCategory.IsFailure.Should().BeTrue();
            subCategory.Error.Description.Should().Be("Заголовок подкатегории не может быть пустым.");
        }

        [Fact]
        public void Create_ShouldGenerateDomainEvent_WhenCreated()
        {
            var subCategory = CreateSubCategory();

            subCategory.DomainEvents.Should().ContainSingle(e => e is SubCategoryCreatedDomainEvent);
        }

        [Fact]
        public void UpdateTitle_ShouldReturnSuccess_WhenTitleIsValid()
        {
            var subCategory = CreateSubCategory();
            var newTitle = "Updated SubCategory";

            var result = subCategory.UpdateTitle(newTitle);

            result.IsSuccess.Should().BeTrue();
            subCategory.Title.Should().Be(newTitle);
        }

        [Fact]
        public void UpdateTitle_ShouldReturnFailure_WhenTitleIsEmpty()
        {
            var subCategory = CreateSubCategory();

            var result = subCategory.UpdateTitle(string.Empty);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Заголовок подкатегории не может быть пустым.");
        }

        [Fact]
        public void UpdateTitle_ShouldGenerateDomainEvent_WhenTitleIsUpdated()
        {
            var subCategory = CreateSubCategory();

            subCategory.UpdateTitle("Updated SubCategory");

            subCategory.DomainEvents.Should().ContainSingle(e => e is SubCategoryTitleUpdatedDomainEvent);
        }

        [Fact]
        public void DecrementLotsCount_ShouldNotDecreaseBelowZero()
        {
            var subCategory = CreateSubCategory();

            var result = subCategory.DecrementLotsCount();

            result.IsFailure.Should().BeTrue();
            subCategory.LotsCount.Should().Be(0);
        }

        [Fact]
        public void CanDelete_ShouldReturnFailure_WhenLotsCountIsGreaterThanZero()
        {
            var subCategory = CreateSubCategory();
            subCategory.IncrementLotsCount();

            var result = subCategory.CanDelete();

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Нельзя удалить подкатегорию, содержащую активные лоты.");
        }

        [Fact]
        public void CanDelete_ShouldReturnSuccess_WhenLotsCountIsZero()
        {
            var subCategory = CreateSubCategory();

            var result = subCategory.CanDelete();

            subCategory.LotsCount.Should().Be(0);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void AddTag_ShouldReturnSuccess_WhenTagIsValid()
        {
            var subCategory = CreateSubCategory();
            var tagId = Guid.NewGuid();

            var result = subCategory.AddTag(tagId);

            result.IsSuccess.Should().BeTrue();
            subCategory.TagsIds.Should().Contain(tagId);
        }

        [Fact]
        public void AddTag_ShouldReturnFailure_WhenTagIsEmpty()
        {
            var subCategory = CreateSubCategory();

            var result = subCategory.AddTag(Guid.Empty);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Тег не может быть пустым.");
        }

        [Fact]
        public void AddTag_ShouldReturnFailure_WhenTagAlreadyExists()
        {
            var subCategory = CreateSubCategory();
            var tagId = Guid.NewGuid();
            subCategory.AddTag(tagId);

            var result = subCategory.AddTag(tagId);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Данный тег уже добавлен к подкатегории.");
        }

        [Fact]
        public void AddTag_ShouldGenerateDomainEvent_WhenTagIsAdded()
        {
            var subCategory = CreateSubCategory();

            subCategory.AddTag(Guid.NewGuid());

            subCategory.DomainEvents.Should().ContainSingle(e => e is SubCategoryTagAddedDomainEvent);
        }

        [Fact]
        public void RemoveTag_ShouldReturnSuccess_WhenTagExists()
        {
            var subCategory = CreateSubCategory();
            var tagId = Guid.NewGuid();
            subCategory.AddTag(tagId);

            var result = subCategory.RemoveTag(tagId);

            result.IsSuccess.Should().BeTrue();
            subCategory.TagsIds.Should().NotContain(tagId);
        }

        [Fact]
        public void RemoveTag_ShouldReturnFailure_WhenTagDoesNotExist()
        {
            var subCategory = CreateSubCategory();

            var result = subCategory.RemoveTag(Guid.NewGuid());

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Тег не найден в подкатегории.");
        }

        [Fact]
        public void RemoveTag_ShouldReturnFailure_WhenTagIsEmpty()
        {
            var subCategory = CreateSubCategory();

            var result = subCategory.RemoveTag(Guid.Empty);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Идентификатор тега не может быть пустым.");
        }

        [Fact]
        public void RemoveTag_ShouldGenerateDomainEvent_WhenTagIsRemoved()
        {
            var subCategory = CreateSubCategory();
            var tagId = Guid.NewGuid();
            subCategory.AddTag(tagId);

            subCategory.RemoveTag(tagId);

            subCategory.DomainEvents.Should().ContainSingle(e => e is SubCategoryTagRemovedDomainEvent);
        }

        [Fact]
        public void LoadTags_ShouldLoadTagsCorrectly()
        {
            var subCategory = CreateSubCategory();
            var tagIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var result = subCategory.LoadTags(tagIds);

            result.IsSuccess.Should().BeTrue();
            subCategory.TagsIds.Should().BeEquivalentTo(tagIds);
        }

        [Fact]
        public void LoadTags_ShouldReturnFailure_WhenTagIdsIsEmpty()
        {
            var subCategory = CreateSubCategory();

            var result = subCategory.LoadTags(new List<Guid>());

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Список идентификаторов тегов не может быть пустым.");
        }

        [Fact]
        public void LoadTags_ShouldReturnFailure_WhenAllTagsIsNull()
        {
            var subCategory = CreateSubCategory();

            var result = subCategory.LoadTags(null!);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Список идентификаторов тегов не может быть пустым.");
        }

        [Fact]
        public void LoadTags_ShouldFilterOutEmptyGuids_AndLoadOnlyValidTags()
        {
            var subCategory = CreateSubCategory();
            var validTagId1 = Guid.NewGuid();
            var validTagId2 = Guid.NewGuid();
            var tagIds = new List<Guid> { validTagId1, Guid.Empty, validTagId2, Guid.Empty };

            var result = subCategory.LoadTags(tagIds);

            result.IsSuccess.Should().BeTrue();
            subCategory.TagsIds.Should().BeEquivalentTo([validTagId1, validTagId2]);
        }

        private static SubCategory CreateSubCategory()
        {
            return SubCategory.Create(Guid.NewGuid(), "Test SubCategory").Value;
        }
    }
}