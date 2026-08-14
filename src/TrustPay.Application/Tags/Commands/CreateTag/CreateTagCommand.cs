namespace TrustPay.Application.Tags.Commands.CreateTag;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;

public record CreateTagCommand(string Name) : IRequest<Result<Guid>>;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<Guid>>
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTagCommandHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var tagResult = Tag.Create(request.Name);
        if (tagResult.IsFailure)
        {
            return Result.Failure<Guid>(tagResult.Error);
        }

        var exists = await _tagRepository.ExistsByNameAsync(tagResult.Value.Name, cancellationToken: cancellationToken);
        if (exists)
        {
            return Result.Failure<Guid>("Тег с таким именем уже существует.");
        }

        await _tagRepository.AddAsync(tagResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tagResult.Value.Id);
    }
}