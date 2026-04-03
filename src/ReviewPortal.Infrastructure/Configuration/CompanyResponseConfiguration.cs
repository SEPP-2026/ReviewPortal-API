using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Infrastructure.Configuration;

public class CompanyResponseConfiguration : IEntityTypeConfiguration<CompanyResponse>
{
    public void Configure(EntityTypeBuilder<CompanyResponse> builder)
    {
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.ResponseText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cr => cr.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(cr => cr.UpdatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // One-to-one: each review has at most one company response
        builder.HasIndex(cr => cr.ReviewId)
            .IsUnique();

        builder.HasOne(cr => cr.Review)
            .WithOne(r => r.CompanyResponse)
            .HasForeignKey<CompanyResponse>(cr => cr.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cr => cr.StaffUser)
            .WithMany(u => u.CompanyResponses)
            .HasForeignKey(cr => cr.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
