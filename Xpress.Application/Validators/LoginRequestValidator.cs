using FluentValidation;
using Xpress.Application.DTOs.Auth;

namespace Xpress.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email address is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}
