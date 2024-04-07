using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CompanyRents.Models;

public class RenewalViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "رقم الاتفاقية")]
    [Required(ErrorMessage = "رقم الاتفاقية")]
    public string AgreementNumber { get; set; } = null!;
    
    [Display(Name = "مدة التجديد (شهور)")]
    [Required(ErrorMessage = "مدة التجديد مطلوبة")]
    public int? RenewalPeriod { get; set; }
    
    [Display(Name = "المبلغ (قبل الضريبة)")]
    [Required(ErrorMessage = "المبلغ مطلوب")]
    public decimal? Amount { get; set; }
    
    [Display(Name = "نسبة الضريبة")]
    [Required(ErrorMessage = "نسبة الضريبة مطلوبة")]
    public decimal? TaxRatio { get; set; }
    
    [Display(Name = "المبلغ (بعد الضريبة)")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "نسبة المشاركة")]
    [Required(ErrorMessage = "نسبة المشاركة مطلوبة")]
    public double? ParticipationRatio { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
    
    [Display(Name = "من")]
    public DateTime StartDate { get; set; }
    
    [Display(Name = "الى")]
    public DateTime EndDate { get; set; }
    
    [Display(Name = "تاريخ التوقيع")]
    public DateTime AgreementDate { get; set; }


    [Display(Name = "من")]
    [Required(ErrorMessage = "من مطلوب")]
    public string StartDateString { get; set; } = null!;
    
    [Display(Name = "الى")]
    [Required(ErrorMessage = "الى مطلوب")]
    public string EndDateString { get; set; }  = null!;
    
    [Display(Name = "تاريخ التوقيع")]
    [Required(ErrorMessage = "تاريخ التوقيع مطلوب")]
    public string AgreementDateString { get; set; }  = null!;
    
    
    [Display(Name = "الملف المرفق")]
    [Required(ErrorMessage = "الملف المرفق مطلوب")]
    public IFormFile? AgreementDocumentFile { get; set; }
    
    public string? AgreementDocumentFileName { get; set; }
    
    public SelectList LessorSelectList { get; set; } = null!;
    
    
    [Display(Name = "رقم العقد")]
    [Required(ErrorMessage = "رقم العقد مطلوب")]
    public int? LessorId { get; set; }
    
}