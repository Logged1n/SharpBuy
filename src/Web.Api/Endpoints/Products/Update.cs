using Application.Abstractions.Messaging;
using Application.Products.Update;
using Domain.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

public sealed class Update : IEndpoint
{
    public sealed record Request(string Name, string Description, decimal PriceAmount, string PriceCurrency);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateProductCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProductCommand(
                id,
                request.Name,
                request.Description,
                request.PriceAmount,
                request.PriceCurrency);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .RequireRoles(Roles.Admin, Roles.Salesman);
    }
}
