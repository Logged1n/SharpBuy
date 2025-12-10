using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.GetAll;
using Domain.Addresses;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Dtos;

namespace Application.Orders.GetById;

internal sealed class GetOrderByIdQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrderByIdQuery, OrderListItem>
{
    public async Task<Result<OrderListItem>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        Order? order = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == query.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderListItem>(OrderErrors.NotFound(query.OrderId));
        }

        var userInfo = await dbContext.DomainUsers
            .Where(du => du.Id == order.UserId)
            .Join(dbContext.ApplicationUsers,
                du => du.Id,
                au => au.DomainUserId,
                (du, au) => new { du.FirstName, du.LastName, au.Email })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        Address? shippingAddress = null;
        Address? billingAddress = null;

        if (order.ShippingAddressId.HasValue)
        {
            shippingAddress = await dbContext.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == order.ShippingAddressId.Value, cancellationToken);
        }

        if (order.BillingAddressId.HasValue)
        {
            billingAddress = await dbContext.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == order.BillingAddressId.Value, cancellationToken);
        }

        return new OrderListItem(
            order.Id,
            order.UserId,
            userInfo?.FirstName ?? "",
            userInfo?.LastName ?? "",
            userInfo?.Email ?? "",
            order.CreatedAt,
            order.ModifiedAt,
            order.CompletedAt,
            order.Status,
            order.ShippingAddressId,
            order.BillingAddressId,
            shippingAddress != null ? new AddressDto(shippingAddress.Line1, shippingAddress.Line2, shippingAddress.City, shippingAddress.PostalCode, shippingAddress.Country) : null,
            billingAddress != null ? new AddressDto(billingAddress.Line1, billingAddress.Line2, billingAddress.City, billingAddress.PostalCode, billingAddress.Country) : null,
            order.Total.Amount,
            order.Total.Currency,
            order.Items.Select(i => new OrderItemDto(
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
    }
}
