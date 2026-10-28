namespace Xpress.Application.DTOs.Transactions;

public sealed record TransactionExportQuery(
    DateTime? From,
    DateTime? To,
    string? Status,
    string? CustomerId
);
