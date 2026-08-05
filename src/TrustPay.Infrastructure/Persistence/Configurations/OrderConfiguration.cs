using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>

    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            
            builder.Property(p => p.Status)
                .IsRequired();
            builder.Property(p => p.Quantity)
                .IsRequired();
            builder.Property(p => p.CreatedAt)
                .IsRequired();
            builder.HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(o => o.Executor)
                .WithMany()
                .HasForeignKey(f => f.ExecutorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(o => o.Lot)
                .WithMany()
                .HasForeignKey(o => o.LotId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Property(o => o.Version)
                 .IsRowVersion();
            
        }
    }
}
