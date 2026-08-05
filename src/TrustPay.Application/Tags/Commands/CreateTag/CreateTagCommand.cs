using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Interfaces;

namespace TrustPay.Application.Tags.Commands.CreateTag
{
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
            string normalizedName = request.Name.Trim().ToUpperInvariant();

            bool exists = await _tagRepository.ExistsByNameAsync(normalizedName, cancellationToken);
            if (exists)
            {
                return Error.Conflict("Tag.AlreadyExists", $"Тег с именем '{request.Name.Trim()}' уже существует.");
            }

            var tagResult = Tag.Create(request.Name);
            if (tagResult.IsFailure)
            {
                return Result.Failure<Guid>(tagResult.Error);
            }

            var tag = tagResult.Value;

            await _tagRepository.AddAsync(tag, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(tag.Id);
        }
    }
}