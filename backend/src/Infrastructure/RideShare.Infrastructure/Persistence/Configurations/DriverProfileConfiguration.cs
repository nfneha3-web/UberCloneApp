using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Configurations;

public class DriverProfileConfiguration : IEntityTypeConfiguration<DriverProfile>
{
    public void Configure(EntityTypeBuilder<DriverProfile> builder)
    {
        builder.ToTable("DriverProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(120);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(x => x.LicenseNumber).IsRequired().HasMaxLength(40);
        builder.Property(x => x.StripeConnectedAccountId).HasMaxLength(100);
        builder.Property(x => x.AvailabilityStatus).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => x.ApplicationUserId).IsUnique();
        builder.HasIndex(x => new { x.LastKnownLatitude, x.LastKnownLongitude });

        builder.Ignore(x => x.DomainEvents);
    }
}
