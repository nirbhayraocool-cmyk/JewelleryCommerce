using JewelleryCommerce.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<AppDbContext>();
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Apply pending migrations
            await db.Database.MigrateAsync();

            // ---------- Roles ----------
            string[] roles = { "Admin", "Vendor", "Customer" };
            foreach (var r in roles)
            {
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));
            }

            // ---------- Admin User ----------
            var adminEmail = "admin@aurajewels.com";
            if (await userMgr.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Super Admin",
                    EmailConfirmed = true
                };
                await userMgr.CreateAsync(admin, "Admin@123");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }

            // ---------- Customer User ----------
            var customerEmail = "customer@aurajewels.com";
            ApplicationUser? customer = await userMgr.FindByEmailAsync(customerEmail);
            if (customer == null)
            {
                customer = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FullName = "Rahul Sharma",
                    EmailConfirmed = true
                };
                await userMgr.CreateAsync(customer, "Customer@123");
                await userMgr.AddToRoleAsync(customer, "Customer");
            }

            // ---------- Categories ----------
            if (!await db.Categories.AnyAsync())
            {
                db.Categories.AddRange(
                    new Category
                    {
                        Name = "Gold",
                        Slug = "gold",
                        DisplayOrder = 1,
                        ImageUrl = "https://images.unsplash.com/photo-1611652022419-a9419f74343d?w=500"
                    },
                    new Category
                    {
                        Name = "Silver",
                        Slug = "silver",
                        DisplayOrder = 2,
                        ImageUrl = "https://images.unsplash.com/photo-1535632066927-ab7c9ab60908?w=500"
                    },
                    new Category
                    {
                        Name = "Diamond",
                        Slug = "diamond",
                        DisplayOrder = 3,
                        ImageUrl = "https://images.unsplash.com/photo-1599643477877-530eb83abc8e?w=500"
                    }
                );
                await db.SaveChangesAsync();
            }

            // ---------- Vendor User + Vendor ----------
            var vendorEmail = "vendor@aurajewels.com";
            var vendorUser = await userMgr.FindByEmailAsync(vendorEmail);
            if (vendorUser == null)
            {
                vendorUser = new ApplicationUser
                {
                    UserName = vendorEmail,
                    Email = vendorEmail,
                    FullName = "Aura Gold House",
                    EmailConfirmed = true
                };
                await userMgr.CreateAsync(vendorUser, "Vendor@123");
                await userMgr.AddToRoleAsync(vendorUser, "Vendor");
            }

            Vendor? vendor = await db.Vendors.FirstOrDefaultAsync(v => v.UserId == vendorUser.Id);
            if (vendor == null)
            {
                vendor = new Vendor
                {
                    UserId = vendorUser.Id,
                    ShopName = "Aura Gold House",
                    Description = "Premium jewellery since 1995",
                    GstNumber = "27ABCDE1234F1Z5",
                    Status = VendorStatus.Approved,
                    CommissionRate = 10m
                };
                db.Vendors.Add(vendor);
                await db.SaveChangesAsync();
            }

            // ---------- Demo Products ----------
            if (!await db.Products.AnyAsync())
            {
                var goldCat = await db.Categories.FirstAsync(c => c.Slug == "gold");
                var silverCat = await db.Categories.FirstAsync(c => c.Slug == "silver");
                var diamondCat = await db.Categories.FirstAsync(c => c.Slug == "diamond");

                for (int i = 1; i <= 12; i++)
                {
                    var product = new Product
                    {
                        VendorId = vendor.Id,
                        CategoryId = i % 3 == 0 ? goldCat.Id
                                    : i % 3 == 1 ? silverCat.Id
                                    : diamondCat.Id,
                        Title = i % 3 == 0 ? $"22K Gold Necklace Set #{i}"
                              : i % 3 == 1 ? $"925 Silver Anklet Pair #{i}"
                              : $"Diamond Solitaire Ring #{i}",
                        Slug = $"product-{i}",
                        Description = "Premium handcrafted jewellery with BIS hallmark certification. " +
                                      "Perfect for weddings, festivals, and everyday elegance.",
                        MetalType = i % 3 == 0 ? MetalType.Gold
                                  : i % 3 == 1 ? MetalType.Silver
                                  : MetalType.Diamond,
                        Purity = i % 3 == 0 ? "22K"
                               : i % 3 == 1 ? "925"
                               : "18K",
                        Weight = 8.5m + i,
                        Price = 25999 + i * 500,
                        Mrp = 29999 + i * 500,
                        Stock = 10 + i,
                        Rating = 4.2 + (i % 8) * 0.1,
                        ReviewCount = 10 + i * 3,
                        IsBestseller = i % 4 == 0,
                        IsApproved = true,
                        IsActive = true,
                        Images = new List<ProductImage>
                        {
                            new ProductImage
                            {
                                ImageUrl = i % 3 == 0
                                    ? "https://images.unsplash.com/photo-1602751584552-8ba73aad10e1?w=400"
                                    : i % 3 == 1
                                        ? "https://images.unsplash.com/photo-1535632066927-ab7c9ab60908?w=400"
                                        : "https://images.unsplash.com/photo-1599643477877-530eb83abc8e?w=400",
                                IsPrimary = true,
                                DisplayOrder = 1
                            }
                        },
                        Variants = new List<ProductVariant>
                        {
                            new ProductVariant { Name = "Small", AdditionalPrice = 0, Stock = 5, IsDefault = true },
                            new ProductVariant { Name = "Medium", AdditionalPrice = 5000, Stock = 4 },
                            new ProductVariant { Name = "Large", AdditionalPrice = 10000, Stock = 3 }
                        }
                    };
                    db.Products.Add(product);
                }
                await db.SaveChangesAsync();
            }

            // ---------- Demo Reviews ----------
            if (!await db.Reviews.AnyAsync() && customer != null)
            {
                var firstProduct = await db.Products.FirstAsync();

                db.Reviews.AddRange(
                    new Review
                    {
                        ProductId = firstProduct.Id,
                        UserId = customer.Id,
                        Rating = 5,
                        Title = "Beautiful craftsmanship!",
                        Comment = "Gold quality bahut achi hai. Hallmark certificate bhi mila. Highly recommended.",
                        IsVerifiedPurchase = true,
                        IsApproved = true
                    },
                    new Review
                    {
                        ProductId = firstProduct.Id,
                        UserId = customer.Id,
                        Rating = 4,
                        Title = "Worth the price",
                        Comment = "Design elegant hai. Delivery time thoda zyada laga but product perfect tha.",
                        IsVerifiedPurchase = true,
                        IsApproved = true
                    }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}