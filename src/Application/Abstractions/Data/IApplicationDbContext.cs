using Domain.Addresses;
using Domain.Carts;
using Domain.Categories;
using Domain.Inventories;
using Domain.Orders;
using Domain.Products;
using Domain.Reviews;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Address> Addresss { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Category> Categories { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Product> Products { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
