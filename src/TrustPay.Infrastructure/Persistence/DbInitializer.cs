namespace TrustPay.Infrastructure.Persistence;

using Microsoft.Extensions.DependencyInjection;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TrustPayDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (context.Users.Any(u => u.Role == UserRole.Admin))
        {
            return;
        }

        var adminEmail = "admin@trustpay.com";
        var passwordHash = passwordHasher.HashPassword("Admin123!");

        var adminResult = User.Create(adminEmail, "Admin", passwordHash, UserRole.Admin);
        if (adminResult.IsSuccess)
        {
            await context.Users.AddAsync(adminResult.Value);
            await context.SaveChangesAsync();
        }
    }
}