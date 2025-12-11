using System.Security.Claims;
using Application.Abstractions.Messaging;
using Application.Users.UpdateProfile;
using SharedKernel;
using SharedKernel.Dtos;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class UpdateProfile : IEndpoint
{
    public sealed record Request(
        string Email,
        string FirstName,
        string LastName,
        AddressDto? Address);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/profile", async (
            Request request,
            ICommandHandler<UpdateUserProfileCommand> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            string? userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Results.Unauthorized();
            }

            var command = new UpdateUserProfileCommand(
                userId,
                request.Email,
                request.FirstName,
                request.LastName,
                request.Address);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
