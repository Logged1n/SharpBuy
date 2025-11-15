using Application.Abstractions.Messaging;
using Application.Users.Register;
using SharedKernel;
using SharedKernel.Dtos;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Register : IEndpoint
{
    public sealed record Request(string Email, string FirstName, string LastName, string Password, string PhoneNumber, AddressDto PrimaryAddress, AddressDto[] Addresses);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
            Request request,
            ICommandHandler<RegisterUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password,
                request.PhoneNumber,
                request.PrimaryAddress,
                request.Addresses);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Created, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
