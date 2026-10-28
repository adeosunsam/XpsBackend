using FluentValidation;
using Xpress.Savings.DTOs;

namespace Xpress.Savings.Validators;

public sealed class CreateSavingsAccountRequestValidator : AbstractValidator<CreateSavingsAccountRequest>
{
    public CreateSavingsAccountRequestValidator()
    {
        RuleFor(x => x.LoanId)
            .GreaterThan(0)
            .WithMessage("LoanId must be a positive integer.");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId must be a positive integer.");
    }
}
