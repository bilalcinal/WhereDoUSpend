using FinanceTracker.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure;

public class AppDbContext : IdentityDbContext<AppUser>
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public DbSet<Category> Categories { get; set; }
	public DbSet<Account> Accounts { get; set; }
	public DbSet<Tag> Tags { get; set; }
	public DbSet<Transaction> Transactions { get; set; }
	public DbSet<TransactionTag> TransactionTags { get; set; }
	public DbSet<Budget> Budgets { get; set; }
	public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
	public DbSet<TransactionLog> TransactionLogs { get; set; }
	public DbSet<RefreshToken> RefreshTokens { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Global query filters for soft delete
		modelBuilder.Entity<Category>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<Account>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<Tag>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<Transaction>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<Budget>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<RecurringTransaction>().HasQueryFilter(e => e.DeletedAt == null);

		// Category configuration
		modelBuilder.Entity<Category>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Name).HasMaxLength(64).IsRequired();
			entity.Property(e => e.UserId).HasMaxLength(128).IsRequired();
			entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
		});

		// Account configuration
		modelBuilder.Entity<Account>(entity =>
		{
			entity.Property(e => e.Name).HasMaxLength(128).IsRequired();
			entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
			entity.Property(e => e.OpeningBalance).HasColumnType("decimal(18,2)");
			entity.Property(e => e.UserId).HasMaxLength(128).IsRequired();
			entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
		});

		// Tag configuration
		modelBuilder.Entity<Tag>(entity =>
		{
			entity.Property(e => e.Name).HasMaxLength(64).IsRequired();
			entity.Property(e => e.UserId).HasMaxLength(128).IsRequired();
			entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
		});

		// Transaction configuration
		modelBuilder.Entity<Transaction>(entity =>
		{
			entity.Property(e => e.UserId).HasMaxLength(128).IsRequired();
			entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
			entity.Property(e => e.Type).IsRequired();
			entity.Property(e => e.Date).IsRequired();
			entity.Property(e => e.Note).HasMaxLength(500);
			entity.HasIndex(e => new { e.UserId, e.Date });

			entity.HasOne(e => e.Category)
				.WithMany(e => e.Transactions)
				.HasForeignKey(e => e.CategoryId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(e => e.Account)
				.WithMany(a => a.Transactions)
				.HasForeignKey(e => e.AccountId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		// Many-to-many Transaction <-> Tag
		modelBuilder.Entity<TransactionTag>(entity =>
		{
			entity.HasKey(x => new { x.TransactionId, x.TagId });
			entity.HasOne(x => x.Transaction)
				.WithMany(t => t.TransactionTags)
				.HasForeignKey(x => x.TransactionId);
			entity.HasOne(x => x.Tag)
				.WithMany(t => t.TransactionTags)
				.HasForeignKey(x => x.TagId);
		});

		// Budget configuration
		modelBuilder.Entity<Budget>(entity =>
		{
			entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
			entity.Property(e => e.UserId).HasMaxLength(128).IsRequired();
			entity.HasIndex(e => new { e.UserId, e.CategoryId, e.Year, e.Month }).IsUnique();
		});

		// Recurring
		modelBuilder.Entity<RecurringTransaction>(entity =>
		{
			entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
			entity.Property(e => e.UserId).HasMaxLength(128).IsRequired();
			entity.HasIndex(e => new { e.UserId, e.NextRunAt });
		});

		// RefreshToken
		modelBuilder.Entity<RefreshToken>(entity =>
		{
			entity.HasIndex(x => new { x.UserId, x.Token }).IsUnique();
		});
	}
} 