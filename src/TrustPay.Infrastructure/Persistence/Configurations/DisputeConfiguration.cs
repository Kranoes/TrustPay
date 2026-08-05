using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class DisputeConfiguration : IEntityTypeConfiguration <Dispute>
    {
        public void Configure(EntityTypeBuilder<Dispute> builder)
        {
            builder.Property(p => p.CreatedAt)
                .IsRequired();
            builder.Property(p => p.Status)
                .IsRequired();
            builder.Property(p => p.ExecutorId)
                .IsRequired();
            builder.Property(p => p.CustomerId)
                .IsRequired();
            builder.Property(p => p.Reason)
                .HasMaxLength(300)
                .IsRequired();
            builder.HasOne(o => o.Order)
                .WithOne(o=>o.Dispute)
                .HasForeignKey<Dispute>(d=>d.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
