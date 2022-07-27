using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using FluentUri;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RockFS.Data.Models;
using RockFS.Models.Auth;

namespace RockFS.Controllers;

public partial class AuthController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterAccount(RegisterModel input)
    {
        var res = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), res, true))
        {
            return View("Register");
        }
        try
        {
            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, input.Password);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                await SendConfirmationEmailAsync(user, input.Email);
                ModelState.AddModelError(string.Empty, "Confirmation email sent. Please check your email.");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return View("Register");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateAccount(EmailModel input)
    {
        var res = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), res, true))
        {
            return View("ResendConfirmEmail");
        }
        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "The specified email address was not found.");
            return View("ResendConfirmEmail");
        }

        if (user.EmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "The email address has already been confirmed.");
            return View("ResendConfirmEmail");
        }

        await SendConfirmationEmailAsync(user, input.Email);
        
        ModelState.AddModelError(string.Empty, "Confirmation email sent. Please check your email.");
        return View("ResendConfirmEmail");
    }
    
        
    public async Task SendConfirmationEmailAsync(RockFsUser user, string email)
    {
        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var uri = FluentUriBuilder
            .From(_config.GetValue<string>("ServerAddress"))
            .Path(Urls.ConfirmUrl)
            .QueryParam("id", userId)
            .QueryParam("code", code)
            .ToString();

        var body = await _renderer.RenderViewAsync("~/Views/Templating/ConfirmationEmail.cshtml",
            HtmlEncoder.Default.Encode(uri));
        
        await _emailSender.SendEmailAsync(email, "Confirm your email", body);
    }
    
    private RockFsUser CreateUser()
    {
        try
        {
            return new RockFsUser();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(RockFsUser)}'. " +
                                                $"Ensure that '{nameof(RockFsUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }
}