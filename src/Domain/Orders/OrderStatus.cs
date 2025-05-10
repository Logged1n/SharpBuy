namespace Domain.Orders;

public enum OrderStatus
{
    Open,
    Confirmed,
    Shipped,
    Arrived,
    Collected,
    Completed,
    Returning,
    Cancelled
}
