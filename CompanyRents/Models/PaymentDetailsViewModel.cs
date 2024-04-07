using System.ComponentModel.DataAnnotations;

namespace CompanyRents.Models;

public class PaymentDetailsViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "اجمالي الفاتورة")]
    public decimal TotalInvoiceAmount { get; set; }
    
    [Display(Name = "المبلغ المسدد")]
    public decimal PaidAmount { get; set; }
    
    [Display(Name = "اجمالي المسدد من الفاتورة")]
    public decimal InvoiceTotalPaidAmount { get; set; }
    
    
    [Display(Name = "المبلغ المتبقي")]
    public decimal RemainingAmount { get; set; }
    
    [Display(Name = "تاريخ السداد")]
    public DateTime Date { get; set; }
    
    [Display(Name = "طريقة السداد")]
    public string PaymentType { get; set; } = null!;
    
    [Display(Name = "رقم الحساب المحول منه")]
    public string BankAccountNumber { get; set; } = null!;

    [Display(Name = "رقم الفاتورة")]
    public string InvoiceNumber { get; set; } = null!;
    
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
    
    
    [Display(Name = "اسم المستخدم")]
    public string UserName { get; set; } = null!;
}