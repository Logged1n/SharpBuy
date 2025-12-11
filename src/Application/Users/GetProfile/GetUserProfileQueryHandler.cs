using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Addresses;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Dtos;

namespace Application.Users.GetProfile;

internal sealed class GetUserProfileQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetUserProfileQuery, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(
        GetUserProfileQuery query,
        CancellationToken cancellationToken)
    {
        DomainUser? user = await dbContext.DomainUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserProfileResponse>(UserErrors.NotFound(query.UserId));
        }

        string userEmail = await dbContext.ApplicationUsers
            .Where(au => au.DomainUserId == user.Id)
            .Select(au => au.Email ?? "")
            .FirstOrDefaultAsync(cancellationToken) ?? "";

        Address? address = await dbContext.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == user.Id, cancellationToken);

        AddressDto? addressDto = address != null
            ? new AddressDto(address.Line1, address.Line2, address.City, address.PostalCode, address.Country)
            : null;

        var response = new UserProfileResponse(
            user.Id,
            userEmail,
            user.FirstName,
            user.LastName,
            address?.Id,
            addressDto);

        return response;
    }
}
