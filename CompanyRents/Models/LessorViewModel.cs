using System.ComponentModel.DataAnnotations;
using Core.Entities;

namespace CompanyRents.Models;

public class LessorViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "اسم الجهة المؤجرة")]
    [Required(ErrorMessage = "اسم الجهة المؤجرة مطلوب")]
    public string Name { get; set; } = null!;
    
    [Display(Name = "نوع العقار")]
    [Required(ErrorMessage = "نوع العقار مطلوب")]
    public string PropertyType { get; set; } = null!;
    
    [Display(Name = "رقم العقد")]
    [Required(ErrorMessage = "رقم العقد مطلوب")]
    public string ContractNumber { get; set; } = null!;
    
    [Display(Name = "مبلغ الايجار (قبل الضريبة)")]
    [Required(ErrorMessage = "مبلغ الايجار مطلوب")]
    public decimal? RentAmount { get; set; }
    
    [Display(Name = "نسبة الضريبة")]
    [Required(ErrorMessage = "نسبة الضريبة مطلوبة")]
    public decimal? RentTaxRatio { get; set; }
    
    [Display(Name = "مبلغ الايجار (بعد الضريبة)")]
    public decimal TotalRentAmount { get; set; }

    [Display(Name = "طريقة دفع الايجار (شهور)")]
    [Required(ErrorMessage = "طريقة دفع الايجار مطلوب")]
    public string RentPaymentPeriodType { get; set; } = null!;
    
    
    [Display(Name = "مدة عقد الايجار")]
    [Required(ErrorMessage = "مدة عقد الايجار مطلوبة")]
    public int RentPeriod { get; set; }
    
    [Display(Name = "نوع المدة")]
    [Required(ErrorMessage = "نوع المدة مطلوبة")]
    public string? PaymentMethod { get; set; }
    
    [Display(Name = "تاريخ تحرير العقد")]
    public DateTime ContractDate { get; set; }
    
    [Display(Name = "تاريخ بداية الايجار")]
    public DateTime RentStartDate { get; set; }
    
    [Display(Name = "تاريخ نهاية الايجار")]
    public DateTime RentEndDate { get; set; }
    
    
    [Display(Name = "تاريخ تحرير العقد")]
    [Required(ErrorMessage = "تاريخ تحرير العقد مطلوب")]
    public string ContractDateString { get; set; } = null!;

    [Display(Name = "تاريخ بداية الايجار")]
    [Required(ErrorMessage = "تاريخ بداية الايجار مطلوب")]
    public string RentStartDateString { get; set; } = null!;
    
    [Display(Name = "تاريخ نهاية الايجار")]
    [Required(ErrorMessage = "تاريخ نهاية الايجار مطلوب")]
    public string RentEndDateString { get; set; }= null!;


    [Display(Name = "رقم صالة الوصول")]
    [Required(ErrorMessage = "رقم صالة الوصول مطلوب")]
    public string ArrivalHallNumber { get; set; } = null!;
    
    
    [Display(Name = "الملف المرفق")]
    [Required(ErrorMessage = "الملف المرفق مطلوب")]
    public IFormFile? ContractDocumentFile { get; set; }
    
    public string? DocumentFileName { get; set; }
    
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
    
    [Display(Name = "المدينة")]
    [Required(ErrorMessage = "المدينة مطلوبة")]
    public string City { get; set; } = null!;

    [Display(Name = "نسبة المشاركة")]
    public List<ParticipationRatioViewModel> ParticipationRatiosLessorViewModels { get; set; } = new();
}