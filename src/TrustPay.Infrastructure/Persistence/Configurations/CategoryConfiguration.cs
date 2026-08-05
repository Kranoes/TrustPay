using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure (EntityTypeBuilder<Category> builder)
        {
            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(p => p.Description)
                .HasMaxLength(150);
            builder.HasIndex(i => i.Type);
            builder.Property(p => p.Type)
                .IsRequired();
            
        }
    }
}
