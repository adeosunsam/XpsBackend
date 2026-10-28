using Xpress.Application.DTOs.Transactions;

namespace Xpress.Application.Interfaces;

public interface ITransactionService
{
    Task<PagedResult<TransactionDto>> GetByCustomerAsync(
        string customerId, int page, int pageSize);
}
