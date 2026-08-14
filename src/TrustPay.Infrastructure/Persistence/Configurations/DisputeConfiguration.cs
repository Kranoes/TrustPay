namespace TrustPay.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> builder)
    {
        builder.ToTable("Disputes");

        builder.HasKey(d => d.Id);

        builder.Ignore(d => d.DomainEvents);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.ResolvedAt)
            .IsRequired(false);

        builder.Property(d => d.CustomerId)
            .IsRequired();

        builder.Property(d => d.ExecutorId)
            .IsRequired();

        builder.Property(d => d.ArbitratorId)
            .IsRequired(false);

        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<Dispute>(d => d.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.CustomerId);
        builder.HasIndex(d => d.ExecutorId);
        builder.HasIndex(d => d.Status);
    }
}