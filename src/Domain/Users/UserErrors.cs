using SharedKernel;

namespace Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found.");

    public static readonly Error Unauthorized = Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found.");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique.");

    public static readonly Error EmailNotVerified = Error.Problem(
        "Users.EmailNotVerified",
        "This account has not verified email.");

    public static Error IdentityFailed(IEnumerable<string> erros) => Error.Failure(
        "Users.IdentityFailed",
        $"Identity operation failed. Errors {string.Join(",\n", erros.Select(e => e))}");

    public static readonly Error EmailAlreadyVerified = Error.Conflict(
        "Users.EmailAlreadyVerified",
        "The email is already verified.");
}
