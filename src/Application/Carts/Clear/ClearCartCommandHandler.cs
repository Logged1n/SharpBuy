using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.Clear;

internal sealed class ClearCartCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<ClearCartCommand>
{
    public async Task<Result> Handle(ClearCartCommand command, CancellationToken cancellationToken)
    {
        Cart? cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerId == command.UserId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure(CartErrors.CartNotFound(command.UserId));
        }

        cart.Items.Clear();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
