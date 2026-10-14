using Microsoft.EntityFrameworkCore;
using SimpleCrm.Api.Models;

namespace SimpleCrm.Api.Data;

/// <summary>
/// DbContext är "porten" till databasen. Varje DbSet blir en tabell.
/// </summary>
public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Här ställer vi in regler för tabellerna (längder, index, relationer).
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.OrgNumber).HasMaxLength(20);
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.Property(c => c.Phone).HasMaxLength(50);
            entity.Property(c => c.City).HasMaxLength(100);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.Property(c => c.Phone).HasMaxLength(50);
            entity.Property(c => c.JobTitle).HasMaxLength(100);

            // Tar man bort en kund tas dess kontakter också bort (cascade delete).
            entity.HasOne(c => c.Customer)
                  .WithMany(c => c.Contacts)
                  .HasForeignKey(c => c.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Deal>(entity =>
        {
            entity.Property(d => d.Title).HasMaxLength(200).IsRequired();
            entity.Property(d => d.Amount).HasPrecision(18, 2);
            // Spara enum som text ("Won") i stället för siffra (3).
            entity.Property(d => d.Stage).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(d => d.Customer)
                  .WithMany(c => c.Deals)
                  .HasForeignKey(d => d.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
            // Två användare får inte ha samma e-post.
            entity.HasIndex(u => u.Email).IsUnique();
        });
    }
}
