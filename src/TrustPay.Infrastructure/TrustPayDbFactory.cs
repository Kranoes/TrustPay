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
        public TrustPayDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
            var connectionString = config.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<TrustPayDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new TrustPayDbContext(optionsBuilder.Options);

        }
    }
}
