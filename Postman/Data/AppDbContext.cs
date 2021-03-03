using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Postman.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<MailEntity> Mails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            optionsBuilder.UseSqlite(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MailEntity>().ToTable("Mails");

            modelBuilder.Entity<MailEntity>().HasData(
                new MailEntity()
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    Receiver = "Alice",
                    Address = 2000,
                    Message = "Hello Alice!",
                },
                new MailEntity()
                {
                    Id = new Guid("33333333-3333-3333-3333-333333333333"),
                    Receiver = "Bob",
                    Address = 3000,
                    Message = "Hello Bob!",
                }            
            );
        }

    }
}
