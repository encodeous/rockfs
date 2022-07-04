using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RockFS.Models.Auth;

namespace RockFS.Controllers;

public partial class AuthController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAccount(LoginModel input)
    {
        var res = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), res, true))
        {
            return View("Login");
        }
        try
        {
            var result = await _signInManager.PasswordSignInAsync(input.Email, input.Password, input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "This account has been locked out. Please contact an administrator.");
                    return View("Login");
                }
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "This account has not been activated yet. Please check your email for the confirmation.");
                    return View("Login");
                }
                ModelState.AddModelError(string.Empty, "Invalid credentials, please check the email and password.");
                return View("Login");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return View("Login");
    }
}