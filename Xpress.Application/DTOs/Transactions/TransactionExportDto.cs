namespace Xpress.Application.DTOs.Transactions;

public sealed record TransactionExportDto(
    int Id,
    string Reference,
    string CustomerId,
    decimal Amount,
    string Status,
    DateTime CreatedAt
);
