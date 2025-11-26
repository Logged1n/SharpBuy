using Application.Abstractions.Authentication;
using Domain.Addresses;
using Domain.Carts;
using Domain.Categories;
using Domain.Inventories;
using Domain.Orders;
using Domain.ProductCategories;
using Domain.Products;
using Domain.Reviews;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> ApplicationUsers { get; }
    DbSet<User> DomainUsers { get; }
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }

    //Workaround to acces IdnetityCore entities in application layer
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
