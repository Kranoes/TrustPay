using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TrustPay.Application.Common.Behaviors;
using TrustPay.Application.Wallets.Commands.FreezeWallet;

namespace TrustPay.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;
            services.AddValidatorsFromAssembly(assembly);
            services.AddMediatR(cfg =>
                {
                cfg.RegisterServicesFromAssembly(assembly);
                    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
                    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                });
            return services;
        }
    }
}
