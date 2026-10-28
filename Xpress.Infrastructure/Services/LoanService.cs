using Microsoft.EntityFrameworkCore;
using Xpress.Application.DTOs.Loans;
using Xpress.Application.DTOs.Transactions;
using Xpress.Application.Interfaces;
using Xpress.Domain.Entities;
using Xpress.Infrastructure.Logger;

namespace Xpress.Infrastructure.Services;

public sealed class LoanService : ILoanService
{
    private readonly IApplicationDbContext _writeCtx;
    private readonly IReadApplicationDbContext _readCtx;
    private readonly ISavingsServiceClient _savingsClient;
    private readonly ILog<LoanService> _logger;

    public LoanService(
        IApplicationDbContext writeCtx,
        IReadApplicationDbContext readCtx,
        ISavingsServiceClient savingsClient,
        ILog<LoanService> logger)
    {
        _writeCtx = writeCtx;
        _readCtx = readCtx;
        _savingsClient = savingsClient;
        _logger = logger;
    }

    public async Task<LoanDto> CreateAsync(CreateLoanRequest request)
    {
        try
        {
            var loan = new Loan
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                TermMonths = request.TermMonths,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
            };

            _writeCtx.Loans.Add(loan);
            await _writeCtx.SaveChangesAsync();

            int? savingsAccountId = null;
            try
            {
                var savingsAccount = await _savingsClient.CreateAccountAsync(loan.Id, loan.CustomerId);
                savingsAccountId = savingsAccount.Id;
            }
            catch (Exception savingsEx)
            {
                _logger.Log(savingsEx);
                _writeCtx.Loans.Remove(loan);
                await _writeCtx.SaveChangesAsync();
                throw new InvalidOperationException(
                    "Loan creation failed: savings service could not provision an account. The loan has been rolled back.",
                    savingsEx);
            }

            return ToDto(loan, savingsAccountId);
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }

    public async Task<LoanDto?> GetByIdAsync(int id)
    {
        try
        {
            var loan = await (
                from l in _readCtx.Loans
                where l.Id == id
                select l
            ).FirstOrDefaultAsync();

            return loan is null ? null : ToDto(loan);
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }

    /// <summary>
    /// Fetches all loans for a customer with their transactions in ONE query (no N+1).
    ///
    /// N+1 (what NOT to do):
    ///   var loans = await _readCtx.Loans.Where(l => l.CustomerId == id).ToListAsync();
    ///   foreach (var loan in loans)
    ///       transactions = await _readCtx.Transactions.Where(t => t.LoanId == loan.Id).ToListAsync();
    ///   — 1 query + N queries = N+1 round-trips.
    ///
    /// No navigation property on Loan is needed — the join is explicit and intentional.
    /// </summary>
    public async Task<IReadOnlyList<LoanWithTransactionsDto>> GetWithTransactionsAsync(int customerId)
    {
        try
        {
            var query =  from l in _readCtx.Loans 
                         where l.CustomerId == customerId
                        join t in _readCtx.Transactions on l.Id equals t.LoanId into trans
                        orderby l.CreatedAt descending
                        select new LoanWithTransactionsDto(
                            l.Id, l.CustomerId, l.Amount, l.TermMonths, l.Status, l.CreatedAt,
                            trans.OrderByDescending(t => t.CreatedAt)
                                    .Select(t => new TransactionDto(t.Id, t.Reference, t.Amount, t.Status, t.CreatedAt))
                                    .ToList());

            var sql = query.ToQueryString();

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }

    private static LoanDto ToDto(Loan l, int? savingsAccountId = null) =>
        new(l.Id, l.CustomerId, l.Amount, l.TermMonths, l.Status, l.CreatedAt, savingsAccountId);
}
