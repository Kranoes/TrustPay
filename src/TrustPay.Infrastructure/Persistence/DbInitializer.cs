namespace TrustPay.Infrastructure.Persistence;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var context = scope.ServiceProvider.GetRequiredService<TrustPayDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (context.Users.Any(u => u.Role == UserRole.Admin))
        {
            return;
        }
        if(string.IsNullOrEmpty(configuration.GetValue<string>("Admin:Email")) || string.IsNullOrEmpty(configuration.GetValue<string>("Admin:Password")))
        {
            logger.LogWarning("Admin is not seeded: set Admin:Email and Admin:Password.");
            return;

        }
        var adminEmail = configuration.GetValue<string>("Admin:Email");

        var passwordHash = passwordHasher.HashPassword(configuration.GetValue<string>("Admin:Password")!);
        var adminResult = User.Create(adminEmail, "Admin", passwordHash, UserRole.Admin);
        if (adminResult.IsSuccess)
        {
            await context.Users.AddAsync(adminResult.Value);
            await context.SaveChangesAsync();
        }
        else 
        {
            logger.LogError("Failed to create admin user.");
            return;
        }
        
    }
}