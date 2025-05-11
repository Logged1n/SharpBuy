using Application.Users.Register;
using MediatR;
using SharedKernel;
using SharedKernel.Dtos;
using Web.API.Extensions;
using Web.API.Infrastructure;

namespace Web.API.Endpoints.Users;

internal sealed class Register : IEndpoint
{
    public sealed record Request(
        string Email,
        string FirstName,
        string LastName,
        string Password,
        AddressDto? PrimaryAddress,
        ICollection<AddressDto> AdditionalAddresses);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password,
                request.PrimaryAddress,
                request.AdditionalAddresses);

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
