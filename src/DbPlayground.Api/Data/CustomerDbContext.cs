using DbPlayground.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DbPlayground.Api.Data;

public class CustomerDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.Name).HasMaxLength(120).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(320).IsRequired();
            entity.Property(customer => customer.Phone).HasMaxLength(40);
            entity.Property(customer => customer.CreatedAtUtc).IsRequired();
            entity.HasIndex(customer => customer.Email).IsUnique();
        });
    }
}