using System.Reflection;
using TrustPay.Application.Common.Logging;
using Serilog.Core;
using Serilog.Events;
namespace TrustPay.Api.Logging
{
    public class SensitiveDestructuringPolicy :  IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            var properties = value.GetType().GetProperties();
            if (!properties.Any(p => p.IsDefined(typeof(SensitiveAttribute))))
            {
                result = null!;
                return false;
            }
            List<LogEventProperty> list = new List<LogEventProperty>();

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute(typeof(SensitiveAttribute)) != null)
                {
                   
                    list.Add(new LogEventProperty(property.Name, new ScalarValue("***")));
                }
                else
                {
                    list.Add(new LogEventProperty(property.Name, propertyValueFactory.CreatePropertyValue(property.GetValue(value),true)));

                }

            }
            var structureValue = new StructureValue(list, value.GetType().Name);
            result = structureValue;
            return true;
        }
    }
}
