namespace Xpress.Application.DTOs.Savings;

public sealed record SavingsAccountDto(
    int Id,
    string AccountNumber,
    int LoanId,
    int CustomerId,
    decimal Balance,
    string Status,
    DateTime CreatedAt
);
