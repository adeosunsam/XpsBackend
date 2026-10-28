namespace Xpress.Domain.Entities;

public sealed class Transaction
{
    public int Id { get; set; }
    public string Reference { get; set; }
    public string CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? LoanId { get; set; }
}
