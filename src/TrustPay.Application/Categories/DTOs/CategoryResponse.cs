using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Enums;

namespace TrustPay.Application.Categories.DTOs
{
    public record CategoryResponse(
        Guid Id,
        string Title,
        string Description,
        CategoryType Type);
        }