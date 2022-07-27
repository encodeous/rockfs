using System.Text;
using System.Text.Encodings.Web;
using FluentUri;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RockFS.Data.Models;
using RockFS.Models.Auth;
using RockFS.Services.Email;
using RockFS.Services.RockFS;

namespace RockFS.Controllers;

public partial class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly SignInManager<RockFsUser> _signInManager;
    private readonly UserManager<RockFsUser> _userManager;
    private readonly IUserStore<RockFsUser> _userStore;
    private readonly IUserEmailStore<RockFsUser> _emailStore;
    private readonly EmailSender _emailSender;
    private readonly IConfiguration _config;
    private readonly RazorRenderer _renderer;
    private readonly StateService _state;
    private readonly RoleService _roles;

    public AuthController(ILogger<AuthController> logger, SignInManager<RockFsUser> signInManager, UserManager<RockFsUser> userManager, IUserStore<RockFsUser> userStore, EmailSender emailSender, IConfiguration config, RazorRenderer renderer, StateService state, RoleService roles)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _emailSender = emailSender;
        _config = config;
        _renderer = renderer;
        _state = state;
        _roles = roles;
    }
    public IActionResult Login()
    {
        return View();
    }
    public IActionResult AccessDenied()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    public IActionResult Register()
    {
        return View();
    }
    public IActionResult ResendConfirmEmail()
    {
        return View();
    }
    public async Task<IActionResult> ConfirmEmail(string id, string code)
    {
        if (id == null || code == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            ViewBag.Error = true;
            ViewData["message"] = $"Unable to load user with ID '{id}'.";
        }
        else
        {
            if (user.EmailConfirmed)
            {
                ViewBag.Error = true;
                ViewData["message"] = "The email has already been confirmed.";
            }
            else
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded && await _state.RequiresSetup())
                {
                    await _roles.AddUserToRoleAsync(user, UserRole.SystemAdministrator);
                    await _state.FinishSetup();
                }
                else
                {
                    if (await _roles.GetHighestRoleAsync(user) == UserRole.Guest)
                    {
                        await _roles.AddUserToRoleAsync(user, UserRole.Guest);
                    }
                }
                ViewBag.Error = !result.Succeeded;
                ViewData["message"] = result.Succeeded ? "Thank you for confirming your email. You may now login." : "Error confirming your email. Try re-sending an email confirmation.";
            }
        }
        return View();
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    public IActionResult ResetPassword(string? code)
    {
        if (code == null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }
        else
        {
            var model = new PasswordResetModel()
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
            };
            return View(model);
        }
    }

    private IUserEmailStore<RockFsUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<RockFsUser>)_userStore;
    }
}