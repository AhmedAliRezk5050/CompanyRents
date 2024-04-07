using Microsoft.AspNetCore.Mvc;
using CompanyRents.Models;

namespace CompanyRents.Controllers.ViewComponents;

public class AdminChangePasswordViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(AdminChangePasswordViewModel model)
    {
        return View(model);
    }
}