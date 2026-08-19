using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("transactions");

            builder.HasKey(t => t.Id);

            builder.OwnsOne(t => t.Amount, amountBuilder =>
            {
                amountBuilder.Property(m => m.Amount)
                    .HasColumnName("Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                amountBuilder.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(t => t.Type)
                .HasConversion<int>()
                .IsRequired();
            builder.Property(x => x.ExternalPaymentId)
            .HasMaxLength(100)
            .IsRequired(false);
            builder.Property(t => t.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.CompletedAt)
                .IsRequired(false);

            builder.Property(t => t.ErrorMessage)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.HasOne<Wallet>()
                .WithMany()
                .HasForeignKey(t => t.SenderWalletId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne<Wallet>()
                .WithMany()
                .HasForeignKey(t => t.ReceiverWalletId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasIndex(t => t.SenderWalletId);
            builder.HasIndex(t => t.ReceiverWalletId);
            builder.HasIndex(t => t.CreatedAt);
        }
    }
}