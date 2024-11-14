using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SignalRtc.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SignalRtc.Storage
{
    public class PgdbJsonContext : DbContext
    {
        public DbSet<ChatJsonModel> ChatJsonModels { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfiguration config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json")
                                  .Build();

            optionsBuilder.UseNpgsql(config.GetConnectionString("PostgresJsonEntityConnectionString"));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatJsonModel>(b =>
            {
                b.HasKey(e => e.ChatJsonModelId);
                b.Property(e => e.ChatJsonModelId).UseIdentityColumn();
            });
        }
    }

    public class ChatJsonModel
    {
        [Key]
        public long ChatJsonModelId { get; set; }

        [Column(TypeName = "jsonb")]
        public JsonDocument ChatSession { get; set; }
    }
}
