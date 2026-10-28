using FluentValidation;
using Xpress.Application.DTOs.Transactions;

namespace Xpress.Application.Validators;

public sealed class TransactionQueryValidator : AbstractValidator<TransactionQuery>
{
    public TransactionQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("customerId is required.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("page must be >= 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("pageSize must be between 1 and 100.");
    }
}
