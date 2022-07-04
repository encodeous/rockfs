using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using FluentUri;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RockFS.Models.Auth;

namespace RockFS.Controllers;

public partial class AuthController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(EmailModel input)
    {
        var res = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), res, true))
        {
            return View("ForgotPassword");
        }
        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // Don't reveal that the user does not exist or is not confirmed
            ModelState.AddModelError(string.Empty, "The email has been sent.");
            return View("ForgotPassword");
        }

        await SendPasswordResetEmailAsync(user, input.Email);

        ModelState.AddModelError(string.Empty, "The email has been sent.");
        return View("ForgotPassword");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(PasswordResetModel input)
    {
        var res = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), res, true))
        {
            return View("ResetPassword", new PasswordResetModel()
            {
                Code = input.Code
            });
        }
        
        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            ModelState.AddModelError(string.Empty, "The credentials have been changed, you may log in now.");
            return View("ResetPassword", new PasswordResetModel()
            {
                Code = input.Code
            });
        }

        var result = await _userManager.ResetPasswordAsync(user, 
            Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(input.Code)),
            input.Password);
        if (result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "The credentials have been changed, you may log in now.");
            return View("ResetPassword", new PasswordResetModel()
            {
                Code = input.Code
            });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View("ResetPassword", new PasswordResetModel()
        {
            Code = input.Code
        });
    }
    
    public async Task SendPasswordResetEmailAsync(IdentityUser user, string email)
    {
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var uri = FluentUriBuilder
            .From(_config.GetValue<string>("ServerAddress"))
            .Path(Urls.ResetPasswordUrl)
            .QueryParam("code", code)
            .ToString();

        var body = await _renderer.RenderViewAsync("~/Views/Templating/PasswordResetEmail.cshtml",
            HtmlEncoder.Default.Encode(uri));
        
        await _emailSender.SendEmailAsync(email, "Confirm your email", body);
    }
}