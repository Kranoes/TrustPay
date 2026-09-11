using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("wallets");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.UserId)
                .IsRequired();

            builder.HasIndex(w => w.UserId)
                .IsUnique();

            builder.Property(w => w.Version)
                .IsRowVersion();

            builder.ComplexProperty(w => w.AvailableBalance, balanceBuilder =>
            {
                balanceBuilder.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                balanceBuilder.Property(m => m.Currency)
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.ComplexProperty(w => w.LockedBalance, balanceBuilder =>
            {
                balanceBuilder.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                balanceBuilder.Property(m => m.Currency)
                    .HasMaxLength(3)
                    .IsRequired();
            });
        }
    }
}