namespace CompanyRents.Models;

public class UserDetails
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    
    public bool IsLockedOut { get; set; }

}