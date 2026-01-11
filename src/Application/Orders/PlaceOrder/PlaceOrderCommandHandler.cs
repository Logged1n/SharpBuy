using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Payments;
using Application.Orders.GetAll;
using Domain.Addresses;
using Domain.Carts;
using Domain.Inventories;
using Domain.Orders;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.PlaceOrder;

internal sealed class PlaceOrderCommandHandler(
    IApplicationDbContext dbContext,
    IPaymentService paymentService) : ICommandHandler<PlaceOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        Result paymentResult = await paymentService.ConfirmPaymentAsync(
            command.PaymentIntentId,
            cancellationToken);

        if (paymentResult.IsFailure)
        {
            return Result.Failure<Guid>(paymentResult.Error);
        }

        Cart? cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerId == command.UserId, cancellationToken);

        if (cart is null || !cart.Items.Any())
        {
            return Result.Failure<Guid>(CartErrors.EmptyCart);
        }

        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        Dictionary<Guid, Product> products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        Dictionary<Guid, Inventory> inventories = await dbContext.Inventories
            .Where(inv => productIds.Contains(inv.ProductId))
            .ToDictionaryAsync(inv => inv.ProductId, cancellationToken);

        Guid? shippingAddressId = command.ShippingAddressId;
        Guid? billingAddressId = command.BillingAddressId;

        if (command.ShippingAddress is not null)
        {
            var shippingAddress = Address.Create(command.ShippingAddress);
            shippingAddress.UserId = command.UserId;

            dbContext.Addresses.Add(shippingAddress);
            await dbContext.SaveChangesAsync(cancellationToken);
            shippingAddressId = shippingAddress.Id;
        }

        if (command.BillingAddress is not null)
        {
            var billingAddress = Address.Create(command.BillingAddress);
            billingAddress.UserId = command.UserId;

            dbContext.Addresses.Add(billingAddress);
            await dbContext.SaveChangesAsync(cancellationToken);
            billingAddressId = billingAddress.Id;
        }

        var order = Order.Create(command.UserId, shippingAddressId!.Value, billingAddressId);

        foreach (CartItem cartItem in cart.Items)
        {
            if (!products.TryGetValue(cartItem.ProductId, out Product? product)
                || !inventories.TryGetValue(cartItem.ProductId, out Inventory? inventory))
            {
                return Result.Failure<Guid>(ProductErrors.ProductNotFound(cartItem.ProductId));
            }

            Result addItemResult = order.AddOrderItem(
                product.Id,
                product.Name,
                product.Price,
                cartItem.Quantity);
            Result removeStockResult = inventory.RemoveStock(cartItem.Quantity);

            if (addItemResult.IsFailure)
                return Result.Failure<Guid>(addItemResult.Error);

            if (removeStockResult.IsFailure)
                return Result.Failure<Guid>(removeStockResult.Error);
        }

        dbContext.Orders.Add(order);
        cart.Clear();

        await dbContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
