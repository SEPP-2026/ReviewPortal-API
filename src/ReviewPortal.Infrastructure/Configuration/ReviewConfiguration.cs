using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Infrastructure.Configuration;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReviewerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.ReviewerEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.ReviewText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.OverallRating)
            .HasPrecision(3, 2);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Reviews_EquipmentRating_Range", "[EquipmentRating] BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Reviews_CustomerServiceRating_Range", "[CustomerServiceRating] BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Reviews_TechnicalSupportRating_Range", "[TechnicalSupportRating] BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Reviews_AfterSalesRating_Range", "[AfterSalesRating] BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Reviews_ValueForMoneyRating_Range", "[ValueForMoneyRating] BETWEEN 1 AND 5");
        });

        builder.HasIndex(r => r.ToolId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.UserId);

        builder.HasOne(r => r.Tool)
            .WithMany(t => t.Reviews)
            .HasForeignKey(r => r.ToolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
