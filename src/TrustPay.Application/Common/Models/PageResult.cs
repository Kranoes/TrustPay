using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Models
{
    public record PageResult<T>(IReadOnlyCollection<T> Items, int PageNumber, int PageSize, int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
