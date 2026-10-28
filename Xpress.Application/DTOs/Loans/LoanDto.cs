namespace Xpress.Application.DTOs.Loans;

public sealed record LoanDto(
    int Id,
    int CustomerId,
    decimal Amount,
    int TermMonths,
    string Status,
    DateTime CreatedAt,
    int? SavingsAccountId = null
);
