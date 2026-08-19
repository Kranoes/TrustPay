using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders");
            builder.HasKey(o => o.Id);

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.Quantity)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.ComplexProperty(o => o.Price, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                priceBuilder.Property(m => m.Currency)
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.ExecutorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Lot>()
                .WithMany()
                .HasForeignKey(o => o.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(o => o.Version)
                .IsRowVersion();
        }
    }
}