using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;
namespace TrustPay.Infrastructure
{
    public class TrustPayDbContext :DbContext , ITrustPayDbContext
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Dispute> Disputes => Set<Dispute>();
        public DbSet<Lot> Lots => Set<Lot>();
        public DbSet<Order> Orders => Set<Order>();

        public DbSet<SubCategory> SubCategories=> Set<SubCategory>();
        public DbSet<Tag> Tags=> Set<Tag>();
        public DbSet<LotTag> LotTags => Set<LotTag>();
        public DbSet<User> Users=> Set<User>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Transaction>Transactions => Set<Transaction>();
        public TrustPayDbContext(DbContextOptions<TrustPayDbContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasPostgresExtension("citext");
            builder.Entity<Wallet>(builder =>
            {
                builder.ComplexProperty(w => w.AvailableBalance);
                builder.ComplexProperty(w => w.LockedBalance);
            });
            builder.Entity<Order>(mbuilder =>
            {
                mbuilder.ComplexProperty(o => o.Price);
            });
            builder.Entity<Lot>(mbuilder =>
            {
                mbuilder.ComplexProperty(l => l.Cost);
            });
            builder.ApplyConfigurationsFromAssembly(typeof(TrustPayDbContext).Assembly);
        }

    }
}
