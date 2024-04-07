namespace Core.Entities;

public class Renewal : BaseEntity
{
    public string AgreementNumber { get; set; } = null!;
    public int RenewalPeriod { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxRatio { get; set; }
    public decimal TotalAmount { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime AgreementDate { get; set; }

    
    public string StartDateString { get; set; } = null!;
    public string EndDateString { get; set; } = null!;
    public string AgreementDateString { get; set; } = null!;
    
    public string? Notes { get; set; }
    
    public string Document { get; set; } = null!;
    
    public bool IsPaid { get; set; }
    
    public int LessorId { get; set; }
    public Lessor Lessor { get; set; } = null!;
    
    public double ParticipationRatio { get; set; }
    
    public List<Invoice> Invoices { get; set; } = null!;
}