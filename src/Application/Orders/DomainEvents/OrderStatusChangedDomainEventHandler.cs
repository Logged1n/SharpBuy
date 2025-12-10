using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Emails;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.DomainEvents;

internal sealed class OrderStatusChangedDomainEventHandler(
    IApplicationDbContext dbContext,
    IEmailService emailService) : IDomainEventHandler<OrderStatusChangedDomainEvent>
{
    public async Task Handle(OrderStatusChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Get order
        Order? order = await dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == domainEvent.OrderId, cancellationToken);

        if (order is null)
        {
            return;
        }

        // Get user
        ApplicationUser? user = await dbContext.ApplicationUsers
            .Include(au => au.DomainUser)
            .FirstOrDefaultAsync(u => u.Id == domainEvent.UserId, cancellationToken);

        if (user is null)
        {
            return;
        }

        // Get status message and description
        (string statusMessage, string statusDescription, string actionMessage) = GetStatusMessages(domainEvent.NewStatus);

        // Build email body
        string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>{statusMessage}</h1>
    </div>

    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <p style='font-size: 16px;'>Dear {user.DomainUser.FirstName} {user.DomainUser.LastName},</p>

        <p style='font-size: 16px;'>{statusDescription}</p>

        <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h2 style='color: #667eea; margin-top: 0;'>Order Details</h2>
            <p><strong>Order ID:</strong> {order.Id}</p>
            <p><strong>Order Date:</strong> {order.CreatedAt:yyyy-MM-dd HH:mm}</p>
            <p><strong>Previous Status:</strong> {GetStatusDisplayName(domainEvent.OldStatus)}</p>
            <p><strong>Current Status:</strong> <span style='color: #667eea; font-weight: bold;'>{GetStatusDisplayName(domainEvent.NewStatus)}</span></p>
        </div>

        <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h3 style='color: #667eea; margin-top: 0;'>Order Items</h3>
            <table style='width: 100%; border-collapse: collapse;'>
                <thead>
                    <tr style='background-color: #f0f0f0;'>
                        <th style='padding: 8px; text-align: left;'>Product</th>
                        <th style='padding: 8px; text-align: center;'>Quantity</th>
                        <th style='padding: 8px; text-align: right;'>Price</th>
                    </tr>
                </thead>
                <tbody>
                    {string.Join("", order.Items.Select(item => $@"
                    <tr>
                        <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{item.ProductName}</td>
                        <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'>{item.TotalPrice.Amount:C} {item.TotalPrice.Currency}</td>
                    </tr>"))}
                </tbody>
            </table>
            <div style='margin-top: 15px; padding-top: 15px; border-top: 2px solid #667eea; text-align: right;'>
                <strong style='font-size: 18px;'>Total: {order.Total.Amount:C} {order.Total.Currency}</strong>
            </div>
        </div>

        {(string.IsNullOrEmpty(actionMessage) ? "" : $@"
        <div style='margin-top: 30px; padding: 20px; background-color: #e8eaf6; border-left: 4px solid #667eea; border-radius: 4px;'>
            <p style='margin: 0; font-size: 14px;'><strong>What's Next?</strong></p>
            <p style='margin: 10px 0 0 0; font-size: 14px;'>{actionMessage}</p>
        </div>")}

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
            $"Order Status Update - {GetStatusDisplayName(domainEvent.NewStatus)}",
            emailBody);
    }

    private static (string statusMessage, string statusDescription, string actionMessage) GetStatusMessages(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Confirmed => (
                "Order Confirmed!",
                "Your order has been confirmed and is now being prepared for shipment.",
                "We will notify you once your order has been shipped with tracking information."
            ),
            OrderStatus.Shipped => (
                "Order Shipped!",
                "Great news! Your order has been shipped and is on its way to you.",
                "You should receive your order within the next few business days. Track your shipment using the tracking number provided separately."
            ),
            OrderStatus.Arrived => (
                "Order Arrived!",
                "Your order has arrived at the delivery location and is ready for pickup or final delivery.",
                "Please check your delivery location or wait for the final delivery attempt."
            ),
            OrderStatus.Collected => (
                "Order Collected!",
                "Your order has been collected and is awaiting your pickup.",
                "Please collect your order at your earliest convenience."
            ),
            OrderStatus.Completed => (
                "Order Completed!",
                "Your order has been successfully completed. We hope you enjoy your purchase!",
                "Thank you for shopping with SharpBuy. We'd love to hear your feedback!"
            ),
            OrderStatus.Returning => (
                "Return in Progress",
                "Your order is being processed for return.",
                "We will notify you once the return has been processed and refund has been issued."
            ),
            OrderStatus.Cancelled => (
                "Order Cancelled",
                "Your order has been cancelled as requested.",
                "If you were charged, a refund will be processed within 5-7 business days."
            ),
            _ => (
                "Order Status Updated",
                "Your order status has been updated.",
                ""
            )
        };
    }

    private static string GetStatusDisplayName(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Open => "Open",
            OrderStatus.Confirmed => "Confirmed",
            OrderStatus.Shipped => "Shipped",
            OrderStatus.Arrived => "Arrived",
            OrderStatus.Collected => "Collected",
            OrderStatus.Completed => "Completed",
            OrderStatus.Returning => "Returning",
            OrderStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }
}
