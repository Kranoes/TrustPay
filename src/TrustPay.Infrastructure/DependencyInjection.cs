using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.BloomFilter;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Interfaces.Webhook;
using TrustPay.Infrastructure.Persistence;
using TrustPay.Infrastructure.Persistence.Interceptors;
using TrustPay.Infrastructure.Persistence.Repositories;
using TrustPay.Infrastructure.Services.Authentication;
using TrustPay.Infrastructure.Services;
using TrustPay.Infrastructure.Services.PaymentGateways;
using TrustPay.Infrastructure.Services.Webhook;
using TrustPay.Infrastructure.Services.Webhook.Options;

namespace TrustPay.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<BankOptions>(configuration.GetSection(BankOptions.SectionName));
            var jwtSettings = new JwtSettings();
            configuration.Bind(JwtSettings.SectionName, jwtSettings);
            if (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < 32)
            {
                throw new InvalidOperationException(
                    "JwtSettings:Secret is not set or shorter than 32 characters. " +
                    "Development: dotnet user-secrets set \"JwtSettings:Secret\" <value> --project src/TrustPay.Api. " +
                    "Production: environment variable JwtSettings__Secret.");
            }
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                        NameClaimType = ClaimTypes.NameIdentifier,
                        RoleClaimType = ClaimTypes.Role
                    };
                });
            services.AddHttpContextAccessor();
            services.AddAuthorization();
            services.AddScoped<DispatchDomainEventsInterceptor>();
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Development: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" <value> --project src/TrustPay.Api. Production: environment variable ConnectionStrings__DefaultConnection.");
            services.AddDbContext<TrustPayDbContext>((sp,options )=>
            {
                var interceptor = sp.GetRequiredService<DispatchDomainEventsInterceptor>();
                options.UseNpgsql(connectionString, npgsqloptions => npgsqloptions.MigrationsAssembly("TrustPay.Infrastructure")).AddInterceptors(interceptor);
            });
            var multiplexer = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!); 
            services.AddSingleton<IConnectionMultiplexer>(multiplexer); 
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IPaymentSignatureValidator, PaymentSignatureValidator>();
            services.AddScoped<ITrustPayDbContext>(provider => provider.GetRequiredService<TrustPayDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ILotRepository, LotRepository>();
            services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
            services.AddScoped<IDisputeRepository, DisputeRepository>();
            services.AddScoped<IUserValidationService, UserValidationService>();

            services.AddTransient<StackExchange.Redis.IDatabase>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
            return services;
        }

    }
}
