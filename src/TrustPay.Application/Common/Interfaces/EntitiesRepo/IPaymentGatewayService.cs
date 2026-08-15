using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Application.Common.Interfaces.EntitiesRepo
{
    public interface IPaymentGatewayService
    {
        Task<PaymentGatewayResult> CreatePaymentFormAsync(Guid transactionId,
            Money amount,
            CancellationToken cancellationToken);
    }

}
