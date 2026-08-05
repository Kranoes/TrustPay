using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder) 
        {
            builder.Property(e => e.UserName).IsRequired();
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.CountOfValuations).IsRequired();
            builder.Property(e => e.UserEmail).IsRequired();
            builder.HasOne(w => w.Wallet)
                .WithOne(u => u.User)
                .HasForeignKey<Wallet>(w=>w.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(u=>u.UserName);


        }
    }
}
 