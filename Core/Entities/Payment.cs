namespace Core.Entities;

public class Payment : BaseEntity
{

    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime Date { get; set; }
    public string PaymentType { get; set; } = null!;
    public string BankAccountNumber { get; set; } = null!;
    public string Document { get; set; } = null!;
    
    public string? Notes { get; set; }
    
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
}