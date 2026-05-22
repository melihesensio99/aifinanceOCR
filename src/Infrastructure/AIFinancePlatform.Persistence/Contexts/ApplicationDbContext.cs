using System;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Domain.Entities;

namespace AIFinancePlatform.Persistence.Contexts;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Receipt> Receipts => Set<Receipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
        });

        // Category Configurations
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.ColorHex).HasMaxLength(10);
        });

        // Transaction Configurations
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Source).HasMaxLength(50);

            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Budget Configurations
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LimitAmount).HasPrecision(18, 2);
            entity.Property(e => e.SpentAmount).HasPrecision(18, 2);

            entity.HasOne(b => b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Subscription Configurations
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Price).HasPrecision(18, 2);

            entity.HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Receipt Configurations
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(200);

            entity.HasOne(r => r.User)
                .WithMany(u => u.Receipts)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed Default Categories
        var foodId = new Guid("11111111-1111-1111-1111-111111111111");
        var transId = new Guid("22222222-2222-2222-2222-222222222222");
        var rentId = new Guid("33333333-3333-3333-3333-333333333333");
        var entId = new Guid("44444444-4444-4444-4444-444444444444");
        var subId = new Guid("55555555-5555-5555-5555-555555555555");
        var incId = new Guid("66666666-6666-6666-6666-666666666666");
        var utilId = new Guid("77777777-7777-7777-7777-777777777777");
        var shopId = new Guid("88888888-8888-8888-8888-888888888888");

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = foodId, Name = "Yemek & Restoran", Icon = "restaurant", ColorHex = "#FF5733", IsDefault = true },
            new Category { Id = transId, Name = "Ulaşım & Yakıt", Icon = "directions_car", ColorHex = "#33FF57", IsDefault = true },
            new Category { Id = rentId, Name = "Kira & Ev Giderleri", Icon = "home", ColorHex = "#3357FF", IsDefault = true },
            new Category { Id = entId, Name = "Eğlence & Aktivite", Icon = "movie", ColorHex = "#F3FF33", IsDefault = true },
            new Category { Id = subId, Name = "Abonelikler", Icon = "subscriptions", ColorHex = "#FF33F3", IsDefault = true },
            new Category { Id = incId, Name = "Gelir / Maaş", Icon = "attach_money", ColorHex = "#33FFF9", IsDefault = true },
            new Category { Id = utilId, Name = "Faturalar & Hizmetler", Icon = "electrical_services", ColorHex = "#A633FF", IsDefault = true },
            new Category { Id = shopId, Name = "Alışveriş", Icon = "shopping_bag", ColorHex = "#FF3386", IsDefault = true }
        );
    }
}
