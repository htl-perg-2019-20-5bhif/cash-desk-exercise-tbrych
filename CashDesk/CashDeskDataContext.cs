using CashDesk.Model;
using Microsoft.EntityFrameworkCore;

namespace CashDesk
{
    public class CashDeskDataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("CashDesk");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>().HasKey(m => m.LastName);
            modelBuilder.Entity<Member>().HasMany<Membership>().WithOne().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Membership>().HasMany<Deposit>().WithOne().OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Member> Members { get; set; }

        public DbSet<Membership> Memberships { get; set; }

        public DbSet<Deposit> Deposits { get; set; }
    }
}
