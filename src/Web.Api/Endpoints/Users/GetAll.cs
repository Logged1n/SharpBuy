using Application.Abstractions.Messaging;
using Application.Users.GetAll;
using Domain.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async (
            int page,
            int pageSize,
            IQueryHandler<GetAllUsersQuery, PagedResult<UserListItem>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllUsersQuery(page, pageSize);

            Result<PagedResult<UserListItem>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireRoles(Roles.Admin);
    }
}
