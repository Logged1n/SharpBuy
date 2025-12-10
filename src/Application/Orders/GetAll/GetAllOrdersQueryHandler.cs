using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Addresses;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Dtos;

namespace Application.Orders.GetAll;

internal sealed class GetAllOrdersQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetAllOrdersQuery, PagedResult<OrderListItem>>
{
    public async Task<Result<PagedResult<OrderListItem>>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken)
    {
        int totalCount = await dbContext.Orders.CountAsync(cancellationToken);

        List<Order> ordersWithIncludes = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = ordersWithIncludes.Select(o => o.UserId).Distinct().ToList();
        var shippingAddressIds = ordersWithIncludes.Select(o => o.ShippingAddressId).Where(id => id.HasValue).Distinct().ToList();
        var billingAddressIds = ordersWithIncludes.Select(o => o.BillingAddressId).Where(id => id.HasValue).Distinct().ToList();
        var allAddressIds = shippingAddressIds.Concat(billingAddressIds).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

        Dictionary<Guid, (string FirstName, string LastName, string Email)> userInfoDict = await dbContext.DomainUsers
            .Where(du => userIds.Contains(du.Id))
            .Join(dbContext.ApplicationUsers,
                du => du.Id,
                au => au.DomainUserId,
                (du, au) => new { du.Id, du.FirstName, du.LastName, Email = au.Email ?? "" })
            .AsNoTracking()
            .ToDictionaryAsync(
                x => x.Id,
                x => (x.FirstName, x.LastName, x.Email),
                cancellationToken);

        Dictionary<Guid, Address> addressDict = await dbContext.Addresses
            .Where(a => allAddressIds.Contains(a.Id))
            .AsNoTracking()
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        var orders = ordersWithIncludes.Select(o =>
        {
            userInfoDict.TryGetValue(o.UserId, out (string FirstName, string LastName, string Email) userInfo);
            addressDict.TryGetValue(o.ShippingAddressId ?? Guid.Empty, out Address? shippingAddress);
            addressDict.TryGetValue(o.BillingAddressId ?? Guid.Empty, out Address? billingAddress);

            return new OrderListItem(
                o.Id,
                o.UserId,
                userInfo.FirstName ?? "",
                userInfo.LastName ?? "",
                userInfo.Email ?? "",
                o.CreatedAt,
                o.ModifiedAt,
                o.CompletedAt,
                o.Status,
                o.ShippingAddressId,
                o.BillingAddressId,
                shippingAddress != null ? new AddressDto(shippingAddress.Line1, shippingAddress.Line2, shippingAddress.City, shippingAddress.PostalCode, shippingAddress.Country) : null,
                billingAddress != null ? new AddressDto(billingAddress.Line1, billingAddress.Line2, billingAddress.City, billingAddress.PostalCode, billingAddress.Country) : null,
                o.Total.Amount,
                o.Total.Currency,
                o.Items.Select(i => new OrderItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.UnitPrice.Amount,
                    i.UnitPrice.Currency,
                    i.Quantity,
                    i.TotalPrice.Amount,
                    i.TotalPrice.Currency
                )).ToList()
            );
        }).ToList();

        return PagedResult<OrderListItem>.Create(orders, query.Page, query.PageSize, totalCount);
    }
}
