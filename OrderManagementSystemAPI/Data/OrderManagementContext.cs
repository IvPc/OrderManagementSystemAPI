using Microsoft.EntityFrameworkCore;
using OrderManagementSystemAPI.Models;

namespace OrderManagementSystemAPI.Data;

public class OrderManagementContext : DbContext
{
    public OrderManagementContext(DbContextOptions<OrderManagementContext> options): base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price);
            entity.Property(e => e.StockQuantity);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount);
            entity.HasMany(e => e.OrderItems)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice);
            entity.Property(e => e.Subtotal);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
        });
    }
}

public static class DataSeeder
{
    public static void SeedData(OrderManagementContext context)
    {
        if (context.Products.Any())
            return;

        var products = new List<Product>
            {
                new Product { Name = "Laptop", Price = 999.99m, StockQuantity = 50 },
                new Product { Name = "Mouse", Price = 29.99m, StockQuantity = 100 },
                new Product { Name = "Keyboard", Price = 79.99m, StockQuantity = 75 },
                new Product { Name = "Monitor", Price = 299.99m, StockQuantity = 40 },
                new Product { Name = "Headphones", Price = 149.99m, StockQuantity = 60 },
                new Product { Name = "USB Cable", Price = 12.99m, StockQuantity = 200 },
                new Product { Name = "Power Bank", Price = 59.99m, StockQuantity = 85 },
                new Product { Name = "Webcam", Price = 89.99m, StockQuantity = 30 },
                new Product { Name = "Tablet", Price = 399.99m, StockQuantity = 25 },
                new Product { Name = "Smartphone", Price = 699.99m, StockQuantity = 45 }
            };

        context.Products.AddRange(products);
        context.SaveChanges();

        // Seed some orders
        var orders = new List<Order>
            {
                new Order
                {
                    OrderDate = DateTime.UtcNow.AddDays(-1),
                    TotalAmount = 1117.97m,               
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 999.99m },
                        new OrderItem { ProductId = 2, Quantity = 4, UnitPrice = 29.99m }
                    }
                },
                new Order
                {
                    OrderDate = DateTime.UtcNow.AddDays(-2),
                    TotalAmount = 412.97m,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { ProductId = 4, Quantity = 1, UnitPrice = 299.99m },
                        new OrderItem { ProductId = 5, Quantity = 1, UnitPrice = 112.99m }
                    }
                },
                new Order
                {
                    OrderDate = DateTime.UtcNow.AddDays(-3),
                    TotalAmount = 89.99m,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { ProductId = 3, Quantity = 1, UnitPrice = 79.99m },
                        new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 10.00m }
                    }
                }
            };

        context.Orders.AddRange(orders);
        context.SaveChanges();
    }
}