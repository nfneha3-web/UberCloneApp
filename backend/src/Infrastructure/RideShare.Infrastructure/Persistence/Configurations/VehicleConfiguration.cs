using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Make).IsRequired().HasMaxLength(60);
        builder.Property(x => x.Model).IsRequired().HasMaxLength(60);
        builder.Property(x => x.Color).HasMaxLength(30);
        builder.Property(x => x.PlateNumber).IsRequired().HasMaxLength(15);
        builder.Property(x => x.VehicleType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => x.DriverProfileId);

        builder.Ignore(x => x.DomainEvents);
    }
}
