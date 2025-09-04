using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data
{
    public class DiscountContext : DbContext
    {
        DbSet<Coupon> Coupons { get; set; } = default;
        public DiscountContext(DbContextOptions<DiscountContext>options) 
            :base(options)
        {

        }
        
    }
    
}
