namespace TrustPay.Application.Common.Models
{
    public record PageResult<T>(IReadOnlyCollection<T> Items, int PageNumber, int PageSize, int TotalCount)
    {
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling(TotalCount / (double)PageSize)
            : 0;

        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1 && TotalPages > 0;
    }
}