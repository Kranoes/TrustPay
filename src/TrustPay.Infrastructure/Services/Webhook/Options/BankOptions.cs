using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Infrastructure.Services.Webhook.Options
{
    public class BankOptions
    {
        public const string SectionName = "BankOptions";
        public string SecretKey { get; init; } = string.Empty;
    }
}
