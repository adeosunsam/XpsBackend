namespace Xpress.Application.DTOs.Transactions;

public sealed record TransactionQuery(
    string CustomerId,
    int Page = 1,
    int PageSize = 20
);
