using System.Security.Claims;
using Application.Abstractions.Messaging;
using Application.Users.GetProfile;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/profile", async (
            IQueryHandler<GetUserProfileQuery, UserProfileResponse> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            string? userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Results.Unauthorized();
            }

            var query = new GetUserProfileQuery(userId);

            Result<UserProfileResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
