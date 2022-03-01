using System;
using FileContextCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VkMusicQuizBot
{
    public class FileDatabase : DbContext, IFileDatabase
    {
        public DbSet<User> Users { get; set; }
        private readonly string path;
        public FileDatabase(string path)
        {
            this.path = path ?? throw new ArgumentNullException(nameof(path));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseFileContextDatabase(path);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .OwnsOne(p => p.Statistic, od =>
                {
                    od.Property<long>("Id");
                    od.HasKey("Id");
                    od.ToTable("database-statistics");
                })
                .ToTable("database");
        }
    }
    public interface IFileDatabase : IDisposable
    {
        public DbSet<User> Users { get; set; }
        public int SaveChanges();
        public Task<int> SaveChangesAsync(CancellationToken token = default);
    }
}
