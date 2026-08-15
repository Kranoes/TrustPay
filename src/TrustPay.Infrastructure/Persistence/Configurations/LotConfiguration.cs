using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;
namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class LotConfiguration : IEntityTypeConfiguration<Lot> 

    {
    public void Configure(EntityTypeBuilder<Lot> builder)
        {
            
            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.ItemsCount)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(l=>l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.OwnsOne(l => l.Cost, costBuilder =>
            {
                costBuilder.Property(m => m.Amount)
                .HasPrecision(18, 2)
                .IsRequired();
                costBuilder.Property(m => m.Currency)
                .HasMaxLength(3)
                .IsRequired();
            });


            builder.HasMany<Tag>()
                .WithMany(m => m.Lots)
                .UsingEntity<Dictionary<string, object>>(
                "LotTag",
                j => j.HasOne<Tag>()
                .WithMany()
                .HasForeignKey("TagId")
                .OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Lot>()
                .WithMany()
                .HasForeignKey("LotId")
                .OnDelete(DeleteBehavior.Cascade)
                );

            builder.HasOne<SubCategory>()
                .WithMany()
                .HasForeignKey(l => l.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
        }
    }
}
