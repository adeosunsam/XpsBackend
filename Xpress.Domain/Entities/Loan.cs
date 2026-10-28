namespace Xpress.Domain.Entities;

public sealed class Loan
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
