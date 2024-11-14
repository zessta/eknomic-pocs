using accounting.Entities;
using Microsoft.EntityFrameworkCore;

namespace accounting.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<MasterTransaction> MasterTransactions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(prop => prop.AccountType).HasConversion<string>();
                entity.Property(prop => prop.Description).HasConversion<string>();
                entity.Property(prop => prop.TransferType).HasConversion<string>();
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(prop => prop.AccountType).HasConversion<string>();
            });

            modelBuilder.Entity<Site>(entity =>
            {
                entity.Property(prop => prop.SiteType).HasConversion<string>();
            });
        }
    }
}
