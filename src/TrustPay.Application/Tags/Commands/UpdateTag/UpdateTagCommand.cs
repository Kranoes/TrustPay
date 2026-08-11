namespace TrustPay.Application.Tags.Commands.UpdateTag;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;
using TrustPay.Domain.Interfaces;

public record UpdateTagCommand(Guid Id, string Name) : IRequest<Result<Unit>>;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, Result<Unit>>
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTagCommandHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
        {
            return Result.Failure<Unit>("Тег не найден.");
        }

        var updateResult = tag.UpdateName(request.Name);
        if (updateResult.IsFailure)
        {
            return Result.Failure<Unit>(updateResult.Error);
        }

        var exists = await _tagRepository.ExistsByNameAsync(tag.Name, tag.Id, cancellationToken);
        if (exists)
        {
            return Result.Failure<Unit>("Тег с таким именем уже существует.");
        }

        _tagRepository.Update(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}