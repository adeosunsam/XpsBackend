using Xpress.Application.DTOs.Transactions;

namespace Xpress.Application.DTOs.Loans;

public sealed record LoanWithTransactionsDto(
    int Id,
    int CustomerId,
    decimal Amount,
    int TermMonths,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<TransactionDto> Transactions
);
