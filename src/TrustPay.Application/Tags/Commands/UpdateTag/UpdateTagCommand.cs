namespace TrustPay.Application.Tags.Commands.UpdateTag;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record UpdateTagCommand(Guid Id, string Name) : IRequest<Result<Unit>>;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, Result<Unit>>
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTagCommandHandler(
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            return Result.Failure<Unit>("Недостаточно прав для выполнения операции.");
        }

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