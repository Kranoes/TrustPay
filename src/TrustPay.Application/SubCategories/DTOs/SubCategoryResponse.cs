using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.SubCategories.DTOs
{
    public record SubCategoryResponse(
    Guid Id,
    Guid CategoryId,
    string Title,
    int LotsCount,
    IReadOnlyCollection<Guid> TagsIds);
}
