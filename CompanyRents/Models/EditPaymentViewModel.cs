using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CompanyRents.Models;

public class EditPaymentViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "المبلغ المسدد")]
    [Required(ErrorMessage = "المبلغ المسدد مطلوب")]
    public decimal? PaidAmount { get; set; }

    [Display(Name = "اجمالي الفاتورة")]
    public decimal InvoiceAmount { get; set; }
    
    [Display(Name = "اجمالي المسدد من الفاتورة")]
    public decimal InvoiceTotalPaidAmount { get; set; }

    [Display(Name = "المبلغ المتبقي من الفاتورة")]
    public decimal RemainingInvoiceAmount { get; set; }

    [Display(Name = "تاريخ السداد")]
    [Required(ErrorMessage = "تاريخ السداد مطلوب")]
    public DateTime? Date { get; set; }


    [Display(Name = "طريقة السداد")]
    [Required(ErrorMessage = "طريقة السداد مطلوبة")]
    public string PaymentType { get; set; } = null!;

    [Display(Name = "رقم الحساب المحول منه")]
    [Required(ErrorMessage = "رقم الحساب المحول منه مطلوب")]
    public string BankAccountNumber { get; set; } = null!;

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
    

    [Display(Name = "الملف المرفق")]
    public IFormFile? PaymentDocumentFile { get; set; }

    public string? PaymentDocumentFileName { get; set; }

    
    [Display(Name = "رقم الفاتورة")]
    [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
    public int InvoiceId { get; set; }

    public SelectList InvoiceSelectList { get; set; } = null!;
}