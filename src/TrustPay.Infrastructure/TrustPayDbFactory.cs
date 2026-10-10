using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;
namespace TrustPay.Infrastructure
{
    public class TrustPayDbFactory : IDesignTimeDbContextFactory<TrustPayDbContext>
    {
        private const string ApiUserSecretsId = "37fbe028-88b8-4790-a4b2-7d15d9b692a7";

        public TrustPayDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets(ApiUserSecretsId)
                .AddEnvironmentVariables()
                .Build();
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection is not set. Run: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" <value> --project src/TrustPay.Api");
            var optionsBuilder = new DbContextOptionsBuilder<TrustPayDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new TrustPayDbContext(optionsBuilder.Options);

        }
    }
}
