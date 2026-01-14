using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetAll;

internal sealed class GetAllUsersQueryHandler(
    IApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager)
    : IQueryHandler<GetAllUsersQuery, PagedResult<UserListItem>>
{
    public async Task<Result<PagedResult<UserListItem>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        IQueryable<DomainUser> usersQuery = dbContext.DomainUsers.AsNoTracking();

        int totalCount = await usersQuery.CountAsync(cancellationToken);

        List<DomainUser> users = await usersQuery
            .OrderBy(u => u.Email)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var userListItems = new List<UserListItem>();

        foreach (DomainUser user in users)
        {
            ApplicationUser? appUser = await userManager.FindByIdAsync(user.Id.ToString());
            IList<string> roles = appUser != null
                ? await userManager.GetRolesAsync(appUser)
                : new List<string>();

            userListItems.Add(new UserListItem(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                appUser?.EmailConfirmed ?? false,
                roles.ToList()));
        }

        return PagedResult<UserListItem>.Create(userListItems, query.Page, query.PageSize, totalCount);
    }
}
