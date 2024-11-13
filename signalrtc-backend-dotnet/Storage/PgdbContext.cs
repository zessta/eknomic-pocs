using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SignalRtc.Models;
using System.Xml;

namespace SignalRtc.Storage
{
    public class PgdbContext : DbContext
    {
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<GroupInfo> GroupInfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfiguration config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json")
                                  .Build();

            optionsBuilder.UseNpgsql(config.GetConnectionString("PostgresEntityConnectionString"));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatSession>(b =>
            {
                b.HasKey(e => e.ChatSessionId);
                b.Property(e => e.ChatSessionId).ValueGeneratedOnAdd();
            });
        }
    }
}
