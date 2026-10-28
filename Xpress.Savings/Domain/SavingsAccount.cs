namespace Xpress.Savings.Domain;

public sealed class SavingsAccount
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = default!;
    public int LoanId { get; set; }
    public int CustomerId { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
