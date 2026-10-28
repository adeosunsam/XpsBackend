namespace Xpress.Application.DTOs.Transactions;
public sealed record TransactionDto(int Id, string Reference, decimal Amount, string Status, DateTime CreatedAt);
