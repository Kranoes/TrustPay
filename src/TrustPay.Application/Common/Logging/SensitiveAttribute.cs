using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Logging
{
    [AttributeUsage(AttributeTargets.Property )]
    public sealed class SensitiveAttribute : Attribute 
    {
    }
}
