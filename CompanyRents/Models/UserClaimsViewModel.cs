using System.Security.Claims;

namespace CompanyRents.Models;

public class UserClaimsViewModel
{
    public string Id { get; set; } = null!;

    public IList<ClaimStatusViewModel> ClaimStatusViewModels { get; set; } = null!;

}