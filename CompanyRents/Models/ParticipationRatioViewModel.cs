using System.ComponentModel.DataAnnotations;

namespace CompanyRents.Models;

public class ParticipationRatioViewModel
{
    [Required(ErrorMessage = "السنة مطلوبة")]
    public int? YearNumber { get; set; }
    
    [Required(ErrorMessage = "النسبة مطلوبة")]
    public double? Ratio { get; set; }
}