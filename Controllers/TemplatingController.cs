using Microsoft.AspNetCore.Mvc;

namespace RockFS.Controllers;

public class TemplatingController : Controller
{
    public IActionResult ConfirmationEmail()
    {
        return View((object)"https://example.com");
    }
    public IActionResult PasswordResetEmail()
    {
        return View((object)"https://example.com");
    }
}