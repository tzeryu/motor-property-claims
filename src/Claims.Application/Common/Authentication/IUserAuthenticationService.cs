namespace Claims.Application.Common.Authentication;

public interface IUserAuthenticationService
{
    Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password
    );
}