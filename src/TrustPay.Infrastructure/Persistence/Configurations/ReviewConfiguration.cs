using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration <Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r=>r.Id);
            builder.Property(r => r.Message)
                .HasMaxLength(200)
                .IsRequired();
            builder.Property(r => r.Title)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<Review>(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(r => r.OrderId)
                .IsUnique();
        }
    }
}
