namespace Core.Entities;

public class Lessor : BaseEntity
{
    public string Name { get; set; } = null!;
    public string PropertyType { get; set; } = null!;
    public string ContractNumber { get; set; } = null!;
    public decimal RentAmount { get; set; }
    public decimal RentTaxRatio { get; set; }

    public decimal TotalRentAmount { get; set; }
    public string RentPaymentPeriodType { get; set; } = null!;
    public int RentPeriod { get; set; }
    public DateTime ContractDate { get; set; }
    public DateTime RentStartDate { get; set; }
    public DateTime RentEndDate { get; set; }

    public string ContractDateString { get; set; } = null!;
    public string RentStartDateString { get; set; } = null!;
    public string RentEndDateString { get; set; } = null!;

    public string ArrivalHallNumber { get; set; } = null!;
    public string ContractDocument { get; set; } = null!;
    public string City { get; set; } = null!;
    public bool IsPaid { get; set; }

    public string? Notes { get; set; }
    
    public List<ParticipationRatio> ParticipationRatios { get; set; } = null!;

    public List<Renewal> Renewals { get; set; } = null!;
    public List<Invoice> Invoices { get; set; } = null!;
}