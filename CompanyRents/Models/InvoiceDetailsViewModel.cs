using System.ComponentModel.DataAnnotations;

namespace CompanyRents.Models;

public class InvoiceDetailsViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "رقم الفاتورة")]
    public string InvoiceNumber { get; set; } = null!;

    [Display(Name = "رقم العقد")]
    public string ContractNumber { get; set; } = null!;
    
    [Display(Name = "من")]
    public DateTime From { get; set; }
    
    [Display(Name = "الى")]
    public DateTime To { get; set; }
    
    [Display(Name = "المبلغ")]
    public decimal Amount { get; set; }
    
    [Display(Name = "رقم الحساب البنكي للسداد")]
    public string BankAccountNumber { get; set; } = null!;
        
    [Display(Name = "المرفق")]
    public string Document { get; set; } = null!;
        
    [Display(Name = "رقم الاتفاقية")]
    public string? AgreementNumber { get; set; }
    
    [Display(Name = "اسم المستخدم")]
    public string UserName { get; set; } = null!;
    
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
    
    public string IsPaid { get; set; } = null!;
}