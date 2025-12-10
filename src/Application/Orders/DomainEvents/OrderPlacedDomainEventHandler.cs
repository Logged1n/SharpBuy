using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Emails;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.DomainEvents;

internal sealed class OrderPlacedDomainEventHandler(
    IApplicationDbContext dbContext,
    IEmailService emailService) : IDomainEventHandler<OrderPlacedDomainEvent>
{
    public async Task Handle(OrderPlacedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Order? order = await dbContext.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == domainEvent.OrderId, cancellationToken);

        if (order is null)
        {
            return;
        }

        ApplicationUser? user = await dbContext.ApplicationUsers
            .Include(au => au.DomainUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == domainEvent.UserId, cancellationToken);

        if (user is null)
        {
            return;
        }

        // Build order summary
        string orderItemsHtml = string.Join("", order.Items.Select(item =>
            $@"<tr>
                <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{item.ProductName}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'>{item.UnitPrice.Amount:C} {item.UnitPrice.Currency}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'>{item.TotalPrice.Amount:C} {item.TotalPrice.Currency}</td>
            </tr>"));

        string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>Thank You for Your Order!</h1>
    </div>

    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <p style='font-size: 16px;'>Dear {user.DomainUser.FirstName} {user.DomainUser.LastName},</p>

        <p style='font-size: 16px;'>Thank you for shopping with SharpBuy! Your order has been successfully placed and is being processed.</p>

        <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h2 style='color: #667eea; margin-top: 0;'>Order Details</h2>
            <p><strong>Order ID:</strong> {order.Id}</p>
            <p><strong>Order Date:</strong> {order.CreatedAt:yyyy-MM-dd HH:mm}</p>
            <p><strong>Status:</strong> {order.Status}</p>
        </div>

        <h3 style='color: #667eea;'>Order Summary</h3>
        <table style='width: 100%; border-collapse: collapse; background-color: white; border-radius: 8px; overflow: hidden;'>
            <thead>
                <tr style='background-color: #667eea; color: white;'>
                    <th style='padding: 12px 8px; text-align: left;'>Product</th>
                    <th style='padding: 12px 8px; text-align: center;'>Quantity</th>
                    <th style='padding: 12px 8px; text-align: right;'>Unit Price</th>
                    <th style='padding: 12px 8px; text-align: right;'>Total</th>
                </tr>
            </thead>
            <tbody>
                {orderItemsHtml}
                <tr style='background-color: #f0f0f0; font-weight: bold;'>
                    <td colspan='3' style='padding: 12px 8px; text-align: right;'>Total Amount:</td>
                    <td style='padding: 12px 8px; text-align: right; color: #667eea; font-size: 18px;'>{order.Total.Amount:C} {order.Total.Currency}</td>
                </tr>
            </tbody>
        </table>

        <div style='margin-top: 30px; padding: 20px; background-color: #e8eaf6; border-left: 4px solid #667eea; border-radius: 4px;'>
            <p style='margin: 0; font-size: 14px;'><strong>What's Next?</strong></p>
            <p style='margin: 10px 0 0 0; font-size: 14px;'>You will receive another email once your order has been shipped. You can track your order status in your account.</p>
        </div>

        <p style='margin-top: 30px; font-size: 14px; color: #666;'>If you have any questions about your order, please don't hesitate to contact our customer support.</p>

        <p style='font-size: 16px; margin-top: 30px;'>Best regards,<br><strong>The SharpBuy Team</strong></p>
    </div>

    <div style='text-align: center; margin-top: 20px; padding: 20px; font-size: 12px; color: #999;'>
        <p>This is an automated message, please do not reply to this email.</p>
        <p>&copy; 2024 SharpBuy. All rights reserved.</p>
    </div>
</body>
</html>";

        await emailService.SendEmailAsync(
            user.DomainUser.Email,
            "Thank You for Your Order - Order Confirmation",
            emailBody);
    }
}
