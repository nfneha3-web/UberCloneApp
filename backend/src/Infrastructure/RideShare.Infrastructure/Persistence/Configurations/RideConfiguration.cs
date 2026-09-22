using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Configurations;

public class RideConfiguration : IEntityTypeConfiguration<Ride>
{
    public void Configure(EntityTypeBuilder<Ride> builder)
    {
        builder.ToTable("Rides");
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Pickup, pickup =>
        {
            pickup.Property(p => p.Latitude).HasColumnName("PickupLatitude");
            pickup.Property(p => p.Longitude).HasColumnName("PickupLongitude");
            pickup.Property(p => p.Address).HasColumnName("PickupAddress").HasMaxLength(300);
        });

        builder.OwnsOne(x => x.Dropoff, dropoff =>
        {
            dropoff.Property(p => p.Latitude).HasColumnName("DropoffLatitude");
            dropoff.Property(p => p.Longitude).HasColumnName("DropoffLongitude");
            dropoff.Property(p => p.Address).HasColumnName("DropoffAddress").HasMaxLength(300);
        });

        builder.OwnsOne(x => x.EstimatedFare, fare =>
        {
            fare.Property(p => p.Amount).HasColumnName("EstimatedFareAmount").HasColumnType("decimal(10,2)");
            fare.Property(p => p.Currency).HasColumnName("EstimatedFareCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(x => x.FinalFare, fare =>
        {
            fare.Property(p => p.Amount).HasColumnName("FinalFareAmount").HasColumnType("decimal(10,2)");
            fare.Property(p => p.Currency).HasColumnName("FinalFareCurrency").HasMaxLength(3);
        });

        builder.Property(x => x.RequestedVehicleType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CancellationReason).HasMaxLength(300);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.RiderProfileId);
        builder.HasIndex(x => x.DriverProfileId);
        builder.HasIndex(x => x.Status);

        builder.Ignore(x => x.DomainEvents);
    }
}
