using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Configurations;

public class CopilotConversationConfiguration : IEntityTypeConfiguration<CopilotConversation>
{
    public void Configure(EntityTypeBuilder<CopilotConversation> builder)
    {
        builder.ToTable("CopilotConversations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(120);

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(x => x.CopilotConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Messages).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.ApplicationUserId);

        builder.Ignore(x => x.DomainEvents);
    }
}

public class CopilotMessageConfiguration : IEntityTypeConfiguration<CopilotMessage>
{
    public void Configure(EntityTypeBuilder<CopilotMessage> builder)
    {
        builder.ToTable("CopilotMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.ToolCallJson).HasColumnType("nvarchar(max)");

        builder.Ignore(x => x.DomainEvents);
    }
}
