namespace Claims.Application.Common.Authentication;

public interface IAccessTokenGenerator
{
    GeneratedAccessToken Generate(AuthenticatedUser user);
}