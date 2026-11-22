namespace Claims.Application.Common.Authentication;

public sealed class LoginHandler
{
    private readonly IUserAuthenticationService _authenticationService;
    private readonly IAccessTokenGenerator _tokenGenerator;

    public LoginHandler(
        IUserAuthenticationService authenticationService,
        IAccessTokenGenerator tokenGenerator)
    {
        _authenticationService = authenticationService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResult?> HandleAsync(LoginRequest request)
    {
        var user = await _authenticationService.AuthenticateAsync(
            request.Email,
            request.Password
        );

        if (user is null)
        {
            return null;
        }

        var generatedToken = _tokenGenerator.Generate(user);

        return new LoginResult(
            AccessToken: generatedToken.Token,
            TokenType: "Bearer",
            ExpiresAtUtc: generatedToken.ExpiresAtUtc,
            UserId: user.UserId,
            Email: user.Email,
            FullName: user.FullName,
            Roles: user.Roles
        );
    }
}