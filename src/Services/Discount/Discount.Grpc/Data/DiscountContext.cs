using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data
{
    public class DiscountContext : DbContext
    {
        public DbSet<Coupon> Coupons { get; set; } = default;
        public DiscountContext(DbContextOptions<DiscountContext>options) 
            :base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coupon>().HasData(
                new Coupon { Id = 1, ProductName = "IPhone X", Description = "IPhone Discount", Amount = 150 },
                new Coupon { Id = 2, ProductName = "Samsung 10", Description = "Samsung Discount", Amount = 100 },

                // Added products as coupons
                new Coupon { Id = 3, ProductName = "Wireless Mouse", Description = "Wireless Mouse Discount", Amount = 50 },
                new Coupon { Id = 4, ProductName = "Mechanical Keyboard", Description = "Mechanical Keyboard Discount", Amount = 75 },
                new Coupon { Id = 5, ProductName = "Blue Product", Description = "Blue Product Discount", Amount = 25 },
                new Coupon { Id = 6, ProductName = "Laptop", Description = "Laptop Discount", Amount = 500 }
            );
        }
    }
    
}
