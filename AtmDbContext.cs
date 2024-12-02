using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ATMapp
{
    public class AtmDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<TransactionInfo> Transactions { get; set; }
        public DbSet<PendingTransfer> PendingTransfers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-LM1CPN7\\SQLEXPRESS;Database=ATMSQL;" +
                "Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
            .HasMany(u => u.Transactions)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.SentTransfers)
                .WithOne(pt => pt.Sender)
                .HasForeignKey(pt => pt.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ReceivedTransfers)
                .WithOne(pt => pt.Recipient)
                .HasForeignKey(pt => pt.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<TransactionInfo>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<TransactionInfo>()
                .HasOne(t => t.Recipient)
                .WithMany()
                .HasForeignKey(t => t.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<PendingTransfer>()
                .HasOne(pt => pt.Sender)
                .WithMany(u => u.SentTransfers)
                .HasForeignKey(pt => pt.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PendingTransfer>()
                .HasOne(pt => pt.Recipient)
                .WithMany(u => u.ReceivedTransfers)
                .HasForeignKey(pt => pt.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}