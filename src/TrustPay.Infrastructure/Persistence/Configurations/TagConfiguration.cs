using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);
            builder.HasIndex(t => t.Name);
            builder.HasIndex(t => t.NormalizedName).IsUnique();
            builder.Property(t => t.NormalizedName)
                .IsRequired()
                .HasMaxLength(50);

        }
    }
}
