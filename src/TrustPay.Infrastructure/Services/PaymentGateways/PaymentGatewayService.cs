using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Infrastructure.Services.PaymentGateways
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        public  Task<PaymentGatewayResult> CreatePaymentFormAsync(
            Guid transactionId,
            Money amount,
            CancellationToken cancellationToken)
        {
            var externalPaymentId = $"ext_{Guid.NewGuid():N}";
            var paymentUrl = $"https://fake-bank.com/checkout/{transactionId}?amount={amount.Amount}&currency={amount.Currency}";

            return Task.FromResult(new PaymentGatewayResult(paymentUrl,externalPaymentId));
        }
    }
}
