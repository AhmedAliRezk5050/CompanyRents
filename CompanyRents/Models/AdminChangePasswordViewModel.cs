using System.ComponentModel.DataAnnotations;

namespace CompanyRents.Models;

public class AdminChangePasswordViewModel
{
    public string UserId { get; set; } = null!;
    
    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [StringLength(100, ErrorMessage = "كلمة مرور غير صالحة", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الجديدة")]
    public string NewPassword { get; set; }  = null!;

    
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [Display(Name = "تاكيد كلمة المرور")]
    [Compare("NewPassword", ErrorMessage = "كلمات مرور غير متطابقة")]
    public string ConfirmPassword { get; set; } = null!;
}