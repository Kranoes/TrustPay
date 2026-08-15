using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class LotTagConfiguration : IEntityTypeConfiguration<LotTag>
    {
        public void Configure(EntityTypeBuilder<LotTag> builder)
        {
            builder.ToTable("lot_tags");
            builder.HasKey(lt => new { lt.LotId, lt.TagId });
            builder.HasOne<Lot>()
                .WithMany()
                .HasForeignKey(lt => lt.LotId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Tag>()
                .WithMany()
                .HasForeignKey(lt => lt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
