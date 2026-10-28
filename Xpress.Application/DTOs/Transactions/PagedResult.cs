namespace Xpress.Application.DTOs.Transactions;
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
