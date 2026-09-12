using FluentAssertions;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.DisputeEvents;

namespace TrustPay.UnitTests.Domain.Entities
{
    public class DisputeTests
    {
        private static readonly Guid DefaultOrderId = Guid.NewGuid();
        private static readonly Guid DefaultCustomerId = Guid.NewGuid();
        private static readonly Guid DefaultExecutorId = Guid.NewGuid();
        private const string DefaultReason = "Невыполнение условий договора";

        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var result = Dispute.Create(DefaultOrderId, DefaultCustomerId, DefaultExecutorId, DefaultReason);

            result.IsSuccess.Should().BeTrue();
            result.Value.OrderId.Should().Be(DefaultOrderId);
            result.Value.CustomerId.Should().Be(DefaultCustomerId);
            result.Value.ExecutorId.Should().Be(DefaultExecutorId);
            result.Value.Reason.Should().Be(DefaultReason);
            result.Value.Status.Should().Be(DisputeStatus.Opened);
            result.Value.ArbitratorId.Should().BeNull();
            result.Value.ResolvedAt.Should().BeNull();
        }

        [Fact]
        public void Create_ShouldTrimReason_WhenReasonHasLeadingOrTrailingWhitespace()
        {
            var rawReason = "   Причина с пробелами   ";

            var result = Dispute.Create(DefaultOrderId, DefaultCustomerId, DefaultExecutorId, rawReason);

            result.IsSuccess.Should().BeTrue();
            result.Value.Reason.Should().Be("Причина с пробелами");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenOrderIdIsEmpty()
        {
            var result = Dispute.Create(Guid.Empty, DefaultCustomerId, DefaultExecutorId, DefaultReason);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Идентификатор заказа не может быть пустым.");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenCustomerIdIsEmpty()
        {
            var result = Dispute.Create(DefaultOrderId, Guid.Empty, DefaultExecutorId, DefaultReason);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Идентификаторы участников должны быть указаны.");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenExecutorIdIsEmpty()
        {
            var result = Dispute.Create(DefaultOrderId, DefaultCustomerId, Guid.Empty, DefaultReason);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Идентификаторы участников должны быть указаны.");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenCustomerAndExecutorAreSame()
        {
            var userId = Guid.NewGuid();

            var result = Dispute.Create(DefaultOrderId, userId, userId, DefaultReason);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Заказчик и исполнитель не могут быть одним человеком.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_ShouldReturnFailure_WhenReasonIsEmpty(string? invalidReason)
        {
            var result = Dispute.Create(DefaultOrderId, DefaultCustomerId, DefaultExecutorId, invalidReason!);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Нельзя создать спор без причины.");
        }

        [Fact]
        public void Create_ShouldGenerateDomainEvent_WhenCreated()
        {
            var dispute = CreateDispute();

            dispute.DomainEvents.Should().ContainSingle(e => e is DisputeOpenedDomainEvent);
        }

        [Fact]
        public void AssignArbitrator_ShouldReturnSuccess_WhenStatusIsOpenedAndArbitratorIdIsValid()
        {
            var dispute = CreateDispute();
            var arbitratorId = Guid.NewGuid();

            var result = dispute.AssignArbitrator(arbitratorId);

            result.IsSuccess.Should().BeTrue();
            dispute.ArbitratorId.Should().Be(arbitratorId);
            dispute.Status.Should().Be(DisputeStatus.UnderReview);
        }

        [Fact]
        public void AssignArbitrator_ShouldReturnFailure_WhenArbitratorIdIsEmpty()
        {
            var dispute = CreateDispute();

            var result = dispute.AssignArbitrator(Guid.Empty);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Указан некорректный ID арбитра.");
        }

        [Fact]
        public void AssignArbitrator_ShouldReturnFailure_WhenStatusIsNotOpened()
        {
            var dispute = CreateDispute();
            dispute.AssignArbitrator(Guid.NewGuid());

            var result = dispute.AssignArbitrator(Guid.NewGuid());

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Назначить арбитра можно только для спора в статусе Opened.");
        }

        [Fact]
        public void AssignArbitrator_ShouldGenerateDomainEvent_WhenAssigned()
        {
            var dispute = CreateDispute();

            dispute.AssignArbitrator(Guid.NewGuid());

            dispute.DomainEvents.Should().ContainSingle(e => e is DisputeArbitratorAssignedDomainEvent);
        }

        [Fact]
        public void ResolveInFavorOfCustomer_ShouldReturnSuccess_WhenRequestedByArbitrator()
        {
            var (dispute, arbitratorId) = CreateDisputeInUnderReviewStatus();

            var result = dispute.ResolveInFavorOfCustomer(arbitratorId, isAdmin: false);

            result.IsSuccess.Should().BeTrue();
            dispute.Status.Should().Be(DisputeStatus.ResolvedForBuyer);
            dispute.ResolvedAt.Should().NotBeNull();
        }

        [Fact]
        public void ResolveInFavorOfCustomer_ShouldReturnSuccess_WhenRequestedByAdmin()
        {
            var (dispute, _) = CreateDisputeInUnderReviewStatus();
            var adminId = Guid.NewGuid();

            var result = dispute.ResolveInFavorOfCustomer(adminId, isAdmin: true);

            result.IsSuccess.Should().BeTrue();
            dispute.Status.Should().Be(DisputeStatus.ResolvedForBuyer);
            dispute.ResolvedAt.Should().NotBeNull();
        }

        [Fact]
        public void ResolveInFavorOfCustomer_ShouldReturnFailure_WhenStatusIsNotUnderReview()
        {
            var dispute = CreateDispute();

            var result = dispute.ResolveInFavorOfCustomer(Guid.NewGuid(), isAdmin: true);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Решение по спору может быть вынесено только во время рассмотрения.");
        }

        [Fact]
        public void ResolveInFavorOfCustomer_ShouldReturnFailure_WhenUserIsNotArbitratorAndNotAdmin()
        {
            var (dispute, _) = CreateDisputeInUnderReviewStatus();
            var randomUser = Guid.NewGuid();

            var result = dispute.ResolveInFavorOfCustomer(randomUser, isAdmin: false);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Вынести решение по спору может только назначенный арбитр или администратор.");
        }

        [Fact]
        public void ResolveInFavorOfCustomer_ShouldGenerateDomainEvent_WhenResolved()
        {
            var (dispute, arbitratorId) = CreateDisputeInUnderReviewStatus();

            dispute.ResolveInFavorOfCustomer(arbitratorId, isAdmin: false);

            dispute.DomainEvents.Should().ContainSingle(e => e is DisputeResolvedInFavorOfCustomerDomainEvent);
        }

        [Fact]
        public void ResolveInFavorOfExecutor_ShouldReturnSuccess_WhenRequestedByArbitrator()
        {
            var (dispute, arbitratorId) = CreateDisputeInUnderReviewStatus();

            var result = dispute.ResolveInFavorOfExecutor(arbitratorId, isAdmin: false);

            result.IsSuccess.Should().BeTrue();
            dispute.Status.Should().Be(DisputeStatus.ResolvedForSeller);
            dispute.ResolvedAt.Should().NotBeNull();
        }

        [Fact]
        public void ResolveInFavorOfExecutor_ShouldReturnSuccess_WhenRequestedByAdmin()
        {
            var (dispute, _) = CreateDisputeInUnderReviewStatus();

            var result = dispute.ResolveInFavorOfExecutor(Guid.NewGuid(), isAdmin: true);

            result.IsSuccess.Should().BeTrue();
            dispute.Status.Should().Be(DisputeStatus.ResolvedForSeller);
            dispute.ResolvedAt.Should().NotBeNull();
        }

        [Fact]
        public void ResolveInFavorOfExecutor_ShouldReturnFailure_WhenStatusIsNotUnderReview()
        {
            var dispute = CreateDispute();

            var result = dispute.ResolveInFavorOfExecutor(Guid.NewGuid(), isAdmin: true);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Решение по спору может быть вынесено только во время рассмотрения.");
        }

        [Fact]
        public void ResolveInFavorOfExecutor_ShouldReturnFailure_WhenUserIsNotArbitratorAndNotAdmin()
        {
            var (dispute, _) = CreateDisputeInUnderReviewStatus();

            var result = dispute.ResolveInFavorOfExecutor(Guid.NewGuid(), isAdmin: false);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Вынести решение по спору может только назначенный арбитр или администратор.");
        }

        [Fact]
        public void ResolveInFavorOfExecutor_ShouldGenerateDomainEvent_WhenResolved()
        {
            var (dispute, arbitratorId) = CreateDisputeInUnderReviewStatus();

            dispute.ResolveInFavorOfExecutor(arbitratorId, isAdmin: false);

            dispute.DomainEvents.Should().ContainSingle(e => e is DisputeResolvedInFavorOfExecutorDomainEvent);
        }

        [Fact]
        public void Cancel_ShouldReturnSuccess_WhenRequestedByCustomer()
        {
            var dispute = CreateDispute();

            var result = dispute.Cancel(DefaultCustomerId, isAdmin: false);

            result.IsSuccess.Should().BeTrue();
            dispute.Status.Should().Be(DisputeStatus.Cancelled);
            dispute.ResolvedAt.Should().NotBeNull();
        }

        [Fact]
        public void Cancel_ShouldReturnSuccess_WhenRequestedByExecutor()
        {
            var dispute = CreateDispute();

            var result = dispute.Cancel(DefaultExecutorId, isAdmin: false);

            result.IsSuccess.Should().BeTrue();
            dispute.Status.Should().Be(DisputeStatus.Cancelled);
            dispute.ResolvedAt.Should().NotBeNull();
        }

        [Fact]
        public void Cancel_ShouldReturnSuccess_WhenRequestedByAdmin()
        {
            var dispute = CreateDispute();

            var result = dispute.Cancel(Guid.NewGuid(), isAdmin: true);

            result.IsSuccess.Should().BeTrue();
            dispute.Status.Should().Be(DisputeStatus.Cancelled);
            dispute.ResolvedAt.Should().NotBeNull();
        }

        [Fact]
        public void Cancel_ShouldReturnFailure_WhenStatusIsAlreadyClosed()
        {
            var (dispute, arbitratorId) = CreateDisputeInUnderReviewStatus();
            dispute.ResolveInFavorOfCustomer(arbitratorId, isAdmin: false);

            var result = dispute.Cancel(DefaultCustomerId, isAdmin: false);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Нельзя отменить уже закрытый или ранее отмененный спор.");
        }

        [Fact]
        public void Cancel_ShouldReturnFailure_WhenUserIsNotParticipantAndNotAdmin()
        {
            var dispute = CreateDispute();

            var result = dispute.Cancel(Guid.NewGuid(), isAdmin: false);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Be("Отменить спор могут только его участники или администратор.");
        }

        [Fact]
        public void Cancel_ShouldGenerateDomainEvent_WhenCancelled()
        {
            var dispute = CreateDispute();

            dispute.Cancel(DefaultCustomerId, isAdmin: false);

            dispute.DomainEvents.Should().ContainSingle(e => e is DisputeCancelledDomainEvent);
        }

        private static Dispute CreateDispute()
        {
            return Dispute.Create(DefaultOrderId, DefaultCustomerId, DefaultExecutorId, DefaultReason).Value;
        }

        private static (Dispute Dispute, Guid ArbitratorId) CreateDisputeInUnderReviewStatus()
        {
            var dispute = CreateDispute();
            var arbitratorId = Guid.NewGuid();
            dispute.AssignArbitrator(arbitratorId);
            return (dispute, arbitratorId);
        }
    }
}