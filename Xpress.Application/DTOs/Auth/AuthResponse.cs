namespace Xpress.Application.DTOs.Auth;
public sealed record AuthResponse(string Token, string Email, string FullName, IEnumerable<string> Roles);
