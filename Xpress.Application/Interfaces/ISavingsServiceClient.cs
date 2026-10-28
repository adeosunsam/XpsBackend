using Xpress.Application.DTOs.Savings;

namespace Xpress.Application.Interfaces;

public interface ISavingsServiceClient
{
    Task<SavingsAccountDto> CreateAccountAsync(int loanId, int customerId);
    Task<SavingsAccountDto?> GetAccountByLoanAsync(int loanId);
}
