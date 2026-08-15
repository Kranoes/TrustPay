using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Interfaces.Webhook
{
    public interface IPaymentSignatureValidator
    {
        bool Validate(string rawBody, string signature);
    }
}
