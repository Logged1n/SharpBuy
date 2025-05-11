using SharedKernel;

namespace Domain.Users;

public static class EmailErrors
{
    public static Error SendingFailed(string exceptionMessage) => Error.Failure(
        "Emails.SendingFailed",
        $"The email could not be sent. Exception message: {exceptionMessage}");

    public static readonly Error InvalidToken = Error.Problem(
        "Emails.InvalidToken",
        "The email verification token is invalid or has expired.");
}
