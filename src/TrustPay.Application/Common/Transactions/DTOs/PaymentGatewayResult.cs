using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Transactions.DTOs
{
    public record PaymentGatewayResult(
    string PaymentUrl,
    string ExternalPaymentId);
}
