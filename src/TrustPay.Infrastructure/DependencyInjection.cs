using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Interfaces;
using TrustPay.Infrastructure.Persistence;
using TrustPay.Infrastructure.Persistence.Interceptors;
using TrustPay.Infrastructure.Persistence.Repositories;
using TrustPay.Infrastructure.Repositories;

namespace TrustPay.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<DispatchDomainEventsInterceptor>();
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<TrustPayDbContext>((sp,options )=>
            {
                var interceptor = sp.GetRequiredService<DispatchDomainEventsInterceptor>();
                options.UseNpgsql(connectionString, npgsqloptions => npgsqloptions.MigrationsAssembly("TrustPay.Infrastructure")).AddInterceptors(interceptor);
            });
            services.AddScoped<ITrustPayDbContext>(provider => provider.GetRequiredService<TrustPayDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ILotRepository, LotRepository>();
            services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
            services.AddScoped<IDisputeRepository, DisputeRepository>();
            return services;
        }

    }
}
