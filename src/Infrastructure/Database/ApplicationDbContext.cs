using System.Collections.Generic;
using System.Reflection.Emit;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Addresses;
using Domain.Carts;
using Domain.Categories;
using Domain.Inventories;
using Domain.Orders;
using Domain.ProductCategories;
using Domain.Products;
using Domain.Reviews;
using Domain.Users;
using Infrastructure.Authentication;
using Infrastructure.DomainEvents;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDomainEventsDispatcher? domainEventsDispatcher = null)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<DomainUser> DomainUsers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        SeedDatabase(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.HasDefaultSchema(Schemas.Default);
        IgnoreDomainEvents(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail

        await PublishDomainEventsAsync();

        int result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    private void SeedDatabase(ModelBuilder builder)
    {
        var adminRoleId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var salesmanRoleId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var clientRoleId = Guid.Parse("10000000-0000-0000-0000-000000000003");

        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole<Guid>
            {
                Id = salesmanRoleId,
                Name = "Salesman",
                NormalizedName = "SALESMAN"
            },
            new IdentityRole<Guid>
            {
                Id = clientRoleId,
                Name = "Client",
                NormalizedName = "CLIENT"
            }
        );
    }


    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
        .ToList();

        await domainEventsDispatcher!.DispatchAsync(domainEvents);
    }

    private static void IgnoreDomainEvents(ModelBuilder modelBuilder)
    {
        IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> entityTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(t => typeof(Entity).IsAssignableFrom(t.ClrType));

        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in entityTypes)
        {
            modelBuilder.Entity(entityType.ClrType)
                .Ignore(nameof(Entity.DomainEvents));
        }
    }
}
