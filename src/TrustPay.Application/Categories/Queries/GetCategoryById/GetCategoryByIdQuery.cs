using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;
using TrustPay.Application.Categories.DTOs;
namespace TrustPay.Application.Categories.Queries.GetCategoryById
{
    public record GetCategoryByIdQuery(Guid id) : IRequest<Result<CategoryResponse>>;
   



}
