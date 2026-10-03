using JewelleryCommerce.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace JewelleryCommerce.Web.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---------- Product ----------
            builder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            builder.Entity<Product>()
                .Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.Mrp).HasColumnType("decimal(18,2)");
            builder.Entity<Product>()
                .Property(p => p.Weight).HasColumnType("decimal(10,2)");

            // ---------- Vendor ----------
            builder.Entity<Vendor>()
                .Property(v => v.CommissionRate).HasColumnType("decimal(5,2)");

            builder.Entity<Vendor>()
                .HasOne(v => v.User)
                .WithOne(u => u.Vendor)
                .HasForeignKey<Vendor>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Category ----------
            builder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            // ---------- ProductVariant ----------
            builder.Entity<ProductVariant>()
                .Property(v => v.AdditionalPrice).HasColumnType("decimal(18,2)");

            builder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Review ----------
            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Order ----------
            builder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.Entity<Order>()
                .Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- OrderItem ----------
            builder.Entity<OrderItem>()
                .Property(oi => oi.Price).HasColumnType("decimal(18,2)");
            builder.Entity<OrderItem>()
                .Property(oi => oi.VendorEarning).HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Vendor)
                .WithMany()
                .HasForeignKey(oi => oi.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}