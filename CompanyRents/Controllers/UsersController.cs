using System.Security.Claims;
using Core.Entities;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyRents.Models;
using CompanyRents.Models;
using Serilog;

namespace CompanyRents.Controllers;

public class UsersController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public UsersController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userDetailsList = await _userManager
                .Users
                .Select(u => new UserDetails
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    IsLockedOut = _userManager.IsLockedOutAsync(u).Result
                })
                .ToListAsync();

            ViewBag.UserId = _userManager.GetUserId(User);

            return View(userDetailsList);
        }
        catch (Exception e)
        {
            TempData["home-temp-msg"] = "unknown-error";
            Log.Error("UsersController - Index[GET] - {@message}", e.Message);
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    [Authorize(Permissions.Claims.Edit)]
    public async Task<IActionResult> ManageUserPermissions(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return NotFound();
            }

            var allClaims = Permissions.GetAllPermissions();
            var userClaims = (await _userManager.GetClaimsAsync(user)).Select(c => c.Value).ToList();

            var viewModel = new UserClaimsViewModel
            {
                Id = userId,
                ClaimStatusViewModels = allClaims.Select(claim => new ClaimStatusViewModel
                {
                    Value = claim,
                    Status = userClaims.Contains(claim)
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception e)
        {
            TempData["users-index-temp-msg"] = "unknown-error";
            Log.Error("UsersController - ManageUserPermissions[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [Authorize(Permissions.Claims.Edit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageUserPermissions(UserClaimsViewModel model)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Remove all user's current claims
            var currentClaims = await _userManager.GetClaimsAsync(user);
            await _userManager.RemoveClaimsAsync(user, currentClaims);

            // Add claims based on the checkboxes that were checked
            var newClaims = model.ClaimStatusViewModels
                .Where(c => c.Status)
                .Select(c => new Claim("Permission", c.Value))
                .ToList();

            var result = await _userManager.AddClaimsAsync(user, newClaims);

            if (!result.Succeeded)
            {
                TempData["users-index-temp-msg"] = "edit-error";
                return RedirectToAction(nameof(Index));
            }

            TempData["users-index-temp-msg"] = "edit-success";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["users-index-temp-msg"] = "unknown-error";
            Log.Error("UsersController - ManageUserPermissions[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var changePasswordResult =
                await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                TempData["home-temp-msg"] = "unknown-error";
                return RedirectToAction("Index", "Home");
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["home-temp-msg"] = "change-password-success";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception e)
        {
            TempData["home-index-temp-msg"] = "unknown-error";
            Log.Error("UsersController - ChangePassword[GET] - {@message}", e.Message);
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Permissions.Users.Edit)]
    public async Task<IActionResult> AdminChangePassword(AdminChangePasswordViewModel model)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return RedirectToAction(nameof(Index));
            }

            // Create a new password hash
            var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, model.NewPassword);

            // Set and save the new password
            user.PasswordHash = newPasswordHash;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["users-index-temp-msg"] = "edit-error";
                return RedirectToAction(nameof(Index));
            }


            TempData["users-index-temp-msg"] = "edit-success";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["users-index-temp-msg"] = "unknown-error";
            Log.Error("UsersController - AdminChangePassword[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Users.Edit)]
    public async Task<IActionResult> Lock(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            var loggedInUseId = _userManager.GetUserId(User);

            if (loggedInUseId == user.Id)
            {
                TempData["users-index-temp-msg"] = "edit-error";
                return RedirectToAction(nameof(Index));
            }

            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(30));
                TempData["users-index-temp-msg"] = "edit-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["users-index-temp-msg"] = "edit-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["users-index-temp-msg"] = "unknown-error";
            Log.Error("UsersController - Lock[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Permissions.Users.Edit)]
    public async Task<IActionResult> UnLock(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["users-index-temp-msg"] = "edit-success";
                return RedirectToAction(nameof(Index));
            }

            TempData["users-index-temp-msg"] = "edit-error";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["users-index-temp-msg"] = "unknown-error";
            Log.Error("UsersController - UnLock[GET] - {@message}", e.Message);
            return RedirectToAction(nameof(Index));
        }
    }
}