using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CompanyRents.Models;

public class EditInvoiceViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "رقم الفاتورة")]
    [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
    public string Number { get; set; } = null!;

    [Display(Name = "رقم الاتفاقية")]
    public string? AgreementNumber { get; set; }
    
    [Display(Name = "تاريخ الفاتورة")]
    [Required(ErrorMessage = "تاريخ الفاتورة مطلوب")]
    public DateTime? InvoiceDate { get; set; }
    
    [Display(Name = "من")]
    [Required(ErrorMessage = "من مطلوب")]
    public DateTime? From { get; set; }
    
    [Display(Name = "الى")]
    [Required(ErrorMessage = "الى مطلوب")]
    public DateTime? To { get; set; }
    
    [Display(Name = "المبلغ")]
    [Required(ErrorMessage = "المبلغ مطلوب")]
    public decimal? Amount { get; set; }
    
    [Display(Name = "رقم الحساب البنكي للسداد")]
    [Required(ErrorMessage = "رقم الحساب البنكي للسداد مطلوب")]
    public string BankAccountNumber { get; set; } = null!;
    
    
    [Display(Name = "الملف المرفق")]
    public IFormFile? InvoiceDocumentFile { get; set; }
   
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
    
    public string? InvoiceDocumentFileName { get; set; }

    [Display(Name = "رقم العقد")]
    [Required(ErrorMessage = "رقم العقد مطلوب")]
    public int? LessorId { get; set; }
    
    public SelectList LessorSelectList { get; set; } = null!;
    
    public int? RenewalId { get; set; }
}