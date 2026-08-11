namespace TrustPay.Application.Tags.Queries.GetTagById;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Tags.DTOs;
using TrustPay.Domain.Common;

public record GetTagByIdQuery(
    Guid Id
) : IRequest<Result<TagResponse>>;

public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, Result<TagResponse>>
{
    private readonly ITrustPayDbContext _context;

    public GetTagByIdQueryHandler(ITrustPayDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TagResponse>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
            .Select(t => new TagResponse(t.Id, t.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (tag is null)
        {
            return Result.Failure<TagResponse>("Тег не найден.");
        }

        return Result.Success(tag);
    }
}