using Xpress.Application.DTOs.Loans;

namespace Xpress.Application.Interfaces;

public interface ILoanService
{
    Task<LoanDto> CreateAsync(CreateLoanRequest request);
    Task<LoanDto?> GetByIdAsync(int id);
    Task<IReadOnlyList<LoanWithTransactionsDto>> GetWithTransactionsAsync(int customerId);
}
