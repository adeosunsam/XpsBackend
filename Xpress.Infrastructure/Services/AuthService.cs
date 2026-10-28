using Microsoft.AspNetCore.Identity;
using Xpress.Application.DTOs.Auth;
using Xpress.Application.Interfaces;
using Xpress.Domain.Constants;
using Xpress.Infrastructure.Identity;
using Xpress.Infrastructure.Logger;

namespace Xpress.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokens;
    private readonly ILog<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokens, ILog<AuthService> logger)
    {
        _userManager = userManager;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            if (!request.Email.Contains('@') || !request.Email.Contains('.'))
                throw new ArgumentException("Invalid email address.", nameof(request.Email));

            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
                throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, Roles.User);

            var token = _tokens.GenerateToken(user.Id, user.Email!, [Roles.User]);
            return new AuthResponse(token, user.Email!, $"{user.FirstName} {user.LastName}", [Roles.User]);
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokens.GenerateToken(user.Id, user.Email!, roles);
            return new AuthResponse(token, user.Email!, $"{user.FirstName} {user.LastName}", roles);
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }
}
