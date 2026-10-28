using Xpress.Savings.DTOs;

namespace Xpress.Savings.Services;

public interface ISavingsAccountService
{
    Task<SavingsAccountDto> CreateAsync(CreateSavingsAccountRequest request);
    Task<SavingsAccountDto?> GetByIdAsync(int id);
    Task<SavingsAccountDto?> GetByLoanAsync(int loanId);
}
