using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.GetCart;

internal sealed class GetCartQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetCartQuery, CartResponse>
{
    public async Task<Result<CartResponse>> Handle(GetCartQuery query, CancellationToken cancellationToken)
    {
        Cart? cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerId == query.UserId, cancellationToken);

        if (cart is null)
        {
            // Return empty cart if not found
            var emptyCart = new CartResponse(
                Guid.Empty,
                query.UserId,
                new List<CartItemResponse>(),
                0,
                "USD");
            return emptyCart;
        }

        List<CartItemResponse> items = new();
        foreach (CartItem item in cart.Items)
        {
            var product = await dbContext.Products
                .Where(p => p.Id == item.ProductId)
                .Select(p => new { p.Name, p.MainPhotoPath })
                .FirstOrDefaultAsync(cancellationToken);

            if (product is not null)
            {
                items.Add(new CartItemResponse(
                    item.ProductId,
                    product.Name,
                    item.Quantity,
                    item.UnitPrice.Amount,
                    item.TotalPrice.Amount,
                    item.UnitPrice.Currency,
                    product.MainPhotoPath));
            }
        }

        var response = new CartResponse(
            cart.OwnerId,
            cart.OwnerId,
            items,
            cart.Total.Amount,
            cart.Total.Currency);

        return response;
    }
}
