using DbPlayground.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DbPlayground.Api.Data;

public class CustomerDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductRule> ProductRules => Set<ProductRule>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.Name).HasMaxLength(120).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(320).IsRequired();
            entity.Property(customer => customer.Phone).HasMaxLength(40);
            entity.Property(customer => customer.BirthDate).HasColumnType("date").IsRequired();
            entity.Property(customer => customer.CreatedAtUtc).IsRequired();
            entity.HasIndex(customer => customer.Email).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name).HasMaxLength(160).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(2000);
            entity.Property(product => product.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(product => product.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<ProductRule>(entity =>
        {
            entity.HasKey(rule => rule.Id);
            entity.HasOne(rule => rule.Product).WithMany(product => product.Rules).HasForeignKey(rule => rule.ProductId);
            entity.Property(rule => rule.ValidFromUtc).IsRequired();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.Id);
            entity.HasOne(order => order.Customer).WithMany().HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(order => order.Product).WithMany().HasForeignKey(order => order.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(order => order.UnitPrice).HasPrecision(18, 2).IsRequired();
            entity.Property(order => order.CreatedAtUtc).IsRequired();
        });

        var productId = 1;
        modelBuilder.Entity<Product>().HasData(new Product
        {
            Id = productId,
            Name = "Restricted Starter Product",
            Description = "A seeded product available to customers aged 18 and over.",
            Price = 49.99m,
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        modelBuilder.Entity<ProductRule>().HasData(new ProductRule
        {
            Id = 1,
            ProductId = productId,
            MinimumAge = 18,
            ValidFromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        });
    }
}