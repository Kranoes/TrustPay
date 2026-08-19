using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TrustPay.Application.Common.Interfaces.Webhook;
using TrustPay.Infrastructure.Services.Webhook.Options;

namespace TrustPay.Infrastructure.Services.Webhook
{
    public class PaymentSignatureValidator : IPaymentSignatureValidator
    {
        private readonly BankOptions _options;
        public PaymentSignatureValidator(IOptions<BankOptions> options)
        {
            _options = options.Value;
        }
        public bool Validate(string rawBody, string signature)
        {
            if (string.IsNullOrWhiteSpace(_options.SecretKey))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(rawBody) || string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }
            var keyBytes = Encoding.UTF8.GetBytes(signature);
            var bodyBytes = Encoding.UTF8.GetBytes(rawBody);
            using var hmac = new HMACSHA256(keyBytes);
            var computedHashBytes = hmac.ComputeHash(bodyBytes);
            var cumputedHex = Convert.ToHexStringLower(computedHashBytes);
            var computedBytes = Encoding.UTF8.GetBytes(cumputedHex);
            var incomingBytes = Encoding.UTF8.GetBytes(signature.ToLowerInvariant());
            if (computedBytes.Length != incomingBytes.Length)
            {
                return false;
            }
            return CryptographicOperations.FixedTimeEquals(computedBytes, incomingBytes);
        }
    }
}
