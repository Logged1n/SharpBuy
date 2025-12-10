using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.UpdateOrderStatus;

internal sealed class UpdateOrderStatusCommandHandler(
    IApplicationDbContext dbContext) : ICommandHandler<UpdateOrderStatusCommand>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        Order? order = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(command.OrderId));
        }

        Result result = order.MoveToStatus(command.NewStatus);

        if (result.IsFailure)
        {
            return result;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
