namespace Core.Entities;

public class Invoice : BaseEntity
{
    public string Number { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal Amount { get; set; }
    public string BankAccountNumber { get; set; } = null!;
    public string Document { get; set; } = null!;

    public string? Notes { get; set; }
    
    public int LessorId { get; set; }
    public Lessor Lessor { get; set; } = null!;
    
    public int? RenewalId { get; set; }
    public Renewal? Renewal { get; set; }

    public List<Payment> Payments { get; set; } = null!;
    
    public bool IsPaid { get; set; }
}