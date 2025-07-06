using FinDepen_Backend.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinDepen_Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().Property(u => u.BalanceAmount).HasDefaultValue(0.0);

            builder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId);

            builder.Entity<Budget>(entity =>
            {
                entity.HasOne(b => b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId);
                
                entity.Property(b => b.RenewalFrequency)
                    .HasConversion<string>();
            });

            builder.Entity<Goal>(entity =>
            {
                entity.HasOne(g => g.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.UserId);
                
                entity.Property(g => g.Priority)
                    .HasConversion<string>();
                    
                entity.Property(g => g.Status)
                    .HasConversion<string>();
            });

            builder.Entity<RecurringTransaction>().ToTable("RecurringTransactions");
        }
    }
}
