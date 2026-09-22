using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Configurations;

public class RiderProfileConfiguration : IEntityTypeConfiguration<RiderProfile>
{
    public void Configure(EntityTypeBuilder<RiderProfile> builder)
    {
        builder.ToTable("RiderProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(120);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(x => x.StripeCustomerId).HasMaxLength(100);

        builder.HasIndex(x => x.ApplicationUserId).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
