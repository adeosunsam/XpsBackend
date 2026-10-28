using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xpress.Application.Interfaces;
using Xpress.Infrastructure.Logger;
using Xpress.Infrastructure.Options;

namespace Xpress.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _opts;
    private readonly ILog<TokenService> _logger;

    public TokenService(IOptions<JwtOptions> opts, ILog<TokenService> logger)
    {
        _opts = opts.Value;
        _logger = logger;
    }

    public string GenerateToken(string userId, string email, IEnumerable<string> roles)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, userId),
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _opts.Issuer,
                audience: _opts.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_opts.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }
}
