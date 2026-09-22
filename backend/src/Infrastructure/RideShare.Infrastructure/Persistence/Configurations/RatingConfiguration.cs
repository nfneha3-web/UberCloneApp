using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable("Ratings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Direction).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Comment).HasMaxLength(500);

        builder.HasIndex(x => new { x.RideId, x.RaterProfileId }).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
