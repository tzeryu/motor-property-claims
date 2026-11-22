using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Claims.Application.Common.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Claims.Infrastructure.Identity;

public sealed class JwtAccessTokenGenerator : IAccessTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;

    public JwtAccessTokenGenerator(
        IOptions<JwtOptions> options,
        TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;

        if (string.IsNullOrWhiteSpace(_options.Issuer) ||
            string.IsNullOrWhiteSpace(_options.Audience) ||
            string.IsNullOrWhiteSpace(_options.SigningKey))
        {
            throw new InvalidOperationException(
                "JWT configuration is incomplete."
            );
        }

        if (Encoding.UTF8.GetByteCount(_options.SigningKey) < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key must contain at least 32 bytes."
            );
        }

        if (_options.AccessTokenMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT access token duration must be greater than zero."
            );
        }
    }

    public GeneratedAccessToken Generate(AuthenticatedUser user)
    {
        var issuedAt = _timeProvider.GetUtcNow();

        var expiresAt = issuedAt.AddMinutes(
            _options.AccessTokenMinutes
        );

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim("role", role));
        }

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SigningKey)
        );

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: signingCredentials
        );

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new GeneratedAccessToken(
            tokenString,
            expiresAt
        );
    }
}