using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Amount, amount =>
        {
            amount.Property(p => p.Amount).HasColumnName("Amount").HasColumnType("decimal(10,2)");
            amount.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.StripePaymentIntentId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ClientSecret).IsRequired().HasMaxLength(200);
        builder.Property(x => x.FailureReason).HasMaxLength(300);

        builder.HasIndex(x => x.RideId).IsUnique();
        builder.HasIndex(x => x.StripePaymentIntentId).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
