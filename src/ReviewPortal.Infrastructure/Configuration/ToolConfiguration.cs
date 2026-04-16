using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Infrastructure.Configuration;

public class ToolConfiguration : IEntityTypeConfiguration<Tool>
{
    public void Configure(EntityTypeBuilder<Tool> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.HourlyRate)
            .HasPrecision(10, 2);

        builder.Property(t => t.DailyRate)
            .HasPrecision(10, 2);

        builder.Property(t => t.WeeklyRate)
            .HasPrecision(10, 2);

        builder.Property(t => t.SpecialNotes)
            .HasMaxLength(1000);

        builder.Property(t => t.DepositAmount)
            .HasPrecision(10, 2);

        builder.Property(t => t.OverallRating)
            .HasPrecision(3, 2);

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        builder.Property(t => t.ReviewCount)
            .HasDefaultValue(0);

        builder.Property(t => t.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.UpdatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => t.IsActive);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Tools)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
