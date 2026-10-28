using Microsoft.EntityFrameworkCore;
using Xpress.Savings.Data;
using Xpress.Savings.Domain;
using Xpress.Savings.DTOs;

namespace Xpress.Savings.Services;

public sealed class SavingsAccountService : ISavingsAccountService
{
    private readonly SavingsDbContext _ctx;

    public SavingsAccountService(SavingsDbContext ctx) => _ctx = ctx;

    public async Task<SavingsAccountDto> CreateAsync(CreateSavingsAccountRequest request)
    {
        var exists = await (
            from s in _ctx.SavingsAccounts
            where s.LoanId == request.LoanId
            select s
        ).AnyAsync();

        if (exists)
            throw new InvalidOperationException(
                $"A savings account already exists for loan {request.LoanId}.");

        var account = new SavingsAccount
        {
            AccountNumber = $"SAV-{request.LoanId:D8}",
            LoanId = request.LoanId,
            CustomerId = request.CustomerId,
            Balance = 0m,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
        };

        _ctx.SavingsAccounts.Add(account);
        await _ctx.SaveChangesAsync();

        return ToDto(account);
    }

    public async Task<SavingsAccountDto?> GetByIdAsync(int id)
    {
        var account = await (
            from s in _ctx.SavingsAccounts
            where s.Id == id
            select s
        ).FirstOrDefaultAsync();

        return account is null ? null : ToDto(account);
    }

    public async Task<SavingsAccountDto?> GetByLoanAsync(int loanId)
    {
        var account = await (
            from s in _ctx.SavingsAccounts
            where s.LoanId == loanId
            select s
        ).FirstOrDefaultAsync();

        return account is null ? null : ToDto(account);
    }

    private static SavingsAccountDto ToDto(SavingsAccount s) =>
        new(s.Id, s.AccountNumber, s.LoanId, s.CustomerId, s.Balance, s.Status, s.CreatedAt);
}
