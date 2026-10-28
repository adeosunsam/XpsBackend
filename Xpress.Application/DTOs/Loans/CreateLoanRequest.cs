namespace Xpress.Application.DTOs.Loans;
public sealed record CreateLoanRequest(int CustomerId, decimal Amount, int TermMonths);
