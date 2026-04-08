using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Infrastructure.Configuration;

public class ToolImageConfiguration : IEntityTypeConfiguration<ToolImage>
{
    public void Configure(EntityTypeBuilder<ToolImage> builder)
    {
        builder.HasKey(ti => ti.Id);

        builder.Property(ti => ti.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ti => ti.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(ti => ti.UploadedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(ti => ti.Tool)
            .WithMany(t => t.Images)
            .HasForeignKey(ti => ti.ToolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
