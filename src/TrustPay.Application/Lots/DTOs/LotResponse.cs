using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Lots.DTOs
{
    public record LotResponse(
        Guid Id,
        Guid UserId,
        Guid SubCategoryId,
        string Title,
        decimal Amount,
        string Currency,
        int ItemsCount
    );
}
