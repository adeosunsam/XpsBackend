using Microsoft.EntityFrameworkCore;
using Xpress.Application.DTOs.Transactions;
using Xpress.Application.Interfaces;
using Xpress.Infrastructure.Logger;

namespace Xpress.Infrastructure.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly IReadApplicationDbContext _readCtx;
    private readonly ILog<TransactionService> _logger;

    public TransactionService(IReadApplicationDbContext readCtx, ILog<TransactionService> logger)
    {
        _readCtx = readCtx;
        _logger = logger;
    }

    public async Task<PagedResult<TransactionDto>> GetByCustomerAsync(string customerId, int page, int pageSize)
    {
        try
        {
            var total = await (
                from t in _readCtx.Transactions
                where t.CustomerId == customerId
                select t
            ).CountAsync();

            var items = await (
                from t in _readCtx.Transactions
                where t.CustomerId == customerId
                orderby t.CreatedAt descending, t.Id descending
                select new TransactionDto(t.Id, t.Reference, t.Amount, t.Status, t.CreatedAt)
            ).Skip((page - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync();

            return new PagedResult<TransactionDto>(items, total, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }
}
