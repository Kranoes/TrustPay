using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class LotConfiguration : IEntityTypeConfiguration<Lot>
    {
        public void Configure(EntityTypeBuilder<Lot> builder)
        {
            builder.ToTable("lots");
            builder.HasKey(l => l.Id);

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.ItemsCount)
                .IsRequired();

            builder.Ignore(l => l.TagIds);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ComplexProperty(l => l.Cost, costBuilder =>
            {
                costBuilder.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                costBuilder.Property(m => m.Currency)
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.HasOne<SubCategory>()
                .WithMany()
                .HasForeignKey(l => l.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}