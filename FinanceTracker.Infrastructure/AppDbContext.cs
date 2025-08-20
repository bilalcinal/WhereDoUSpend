using FinanceTracker.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(64).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(128).IsRequired();
            
            // Unique index on UserId + Name combination
            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(128).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Note).HasMaxLength(500);
            
            // Foreign key to Category
            entity.HasOne(e => e.Category)
                  .WithMany(e => e.Transactions)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Index on UserId + Date for efficient queries
            entity.HasIndex(e => new { e.UserId, e.Date });
        });
    }
} 