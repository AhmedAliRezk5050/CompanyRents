namespace Core.Entities;

using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser
{
    public List<Lessor> Lessors { get; set; } = null!;
    public List<Renewal> Renewals { get; set; } = null!;
    public List<Invoice> Invoices { get; set; } = null!;
    
    public List<Payment> Payments { get; set; } = null!;
}