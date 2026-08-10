namespace TrustPay.Application.Disputes.Commands.CreateDispute;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Models;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;

public record CreateDisputeCommand(
    Guid OrderId,
    Guid CustomerId,
    Guid ExecutorId,
    string Reason
) : IRequest<Result<Guid>>;

public class CreateDisputeCommandHandler : IRequestHandler<CreateDisputeCommand, Result<Guid>>
{
    private readonly IDisputeRepository _disputeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDisputeCommandHandler(IDisputeRepository disputeRepository, IUnitOfWork unitOfWork)
    {
        _disputeRepository = disputeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateDisputeCommand request, CancellationToken cancellationToken)
    {
        var disputeResult = Dispute.Create(
            request.OrderId,
            request.CustomerId,
            request.ExecutorId,
            request.Reason);

        if (disputeResult.IsFailure)
        {
            return Result<Guid>.Failure(disputeResult.Error);
        }

        var dispute = disputeResult.Value;

        var arbitratorId = await _disputeRepository.GetAvailableArbitratorIdAsync(cancellationToken);
        if (arbitratorId.HasValue)
        {
            dispute.AssignArbitrator(arbitratorId.Value);
        }

        await _disputeRepository.AddAsync(dispute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(dispute.Id);
    }
}