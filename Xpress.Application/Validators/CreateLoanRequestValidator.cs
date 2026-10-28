using FluentValidation;
using Xpress.Application.DTOs.Loans;

namespace Xpress.Application.Validators;

public sealed class CreateLoanRequestValidator : AbstractValidator<CreateLoanRequest>
{
    public CreateLoanRequestValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("CustomerId must be a positive integer.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.TermMonths).GreaterThan(0).WithMessage("TermMonths must be greater than zero.");
    }
}
