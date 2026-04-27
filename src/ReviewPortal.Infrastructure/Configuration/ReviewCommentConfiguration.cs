using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Infrastructure.Configuration;

public class ReviewCommentConfiguration : IEntityTypeConfiguration<ReviewComment>
{
    public void Configure(EntityTypeBuilder<ReviewComment> builder)
    {
        builder.HasKey(rc => rc.Id);

        builder.Property(rc => rc.CommenterName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rc => rc.CommentText)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(rc => rc.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(rc => rc.RejectionReason)
            .HasMaxLength(500);

        builder.Property(rc => rc.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(rc => rc.Status);

        builder.HasOne(rc => rc.Review)
            .WithMany(r => r.Comments)
            .HasForeignKey(rc => rc.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rc => rc.User)
            .WithMany(u => u.ReviewComments)
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
