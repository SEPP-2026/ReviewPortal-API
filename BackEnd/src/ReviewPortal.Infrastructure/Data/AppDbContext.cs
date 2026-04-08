using Microsoft.EntityFrameworkCore;
using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Tool> Tools => Set<Tool>();

    public DbSet<ToolImage> ToolImages => Set<ToolImage>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();

    public DbSet<CompanyResponse> CompanyResponses => Set<CompanyResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration classes from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set UpdatedDate on modified AuditableEntity entries
        foreach (var entry in ChangeTracker.Entries<Domain.Common.AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.UtcNow;
                entry.Entity.UpdatedDate = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
