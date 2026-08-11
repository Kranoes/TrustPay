namespace TrustPay.Application.Tags.Queries.GetAllTags;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Tags.DTOs;
using TrustPay.Domain.Common;

public record GetAllTagsQuery(string? SearchTerm = null) : IRequest<Result<List<TagResponse>>>;

public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, Result<List<TagResponse>>>
{
    private readonly ITrustPayDbContext _context;

    public GetAllTagsQueryHandler(ITrustPayDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TagResponse>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tags.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            query = query.Where(t => t.Name.Contains(searchTerm));
        }

        var tags = await query
            .Select(t => new TagResponse(t.Id, t.Name))
            .ToListAsync(cancellationToken);

        return Result.Success(tags);
    }
}