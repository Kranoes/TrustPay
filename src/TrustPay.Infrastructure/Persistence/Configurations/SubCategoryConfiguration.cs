using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class SubCategoryConfiguration : IEntityTypeConfiguration <SubCategory>
    {
        public void Configure(EntityTypeBuilder<SubCategory> builder)
        {
            builder.Property(s => s.Title)
                .HasMaxLength(100)
                .IsRequired();
            builder.HasIndex(s=>s.Title);
            builder.HasMany<Tag>()
                .WithMany()
                .UsingEntity<Dictionary<string, object>>("SubCategoryTag",
                j => j.HasOne<Tag>()
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict)
                .HasForeignKey("TagId"),
                j => j.HasOne<SubCategory>()
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey("SubCategoryId")
                );
            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(s=>s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
