using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using RockFS.Data;
using RockFS.Data.Models;

namespace RockFS.Builders;

public static class AuthBuilder
{
    public static void ConfigureAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddIdentityCookies(o => { });

        builder.Services.AddIdentityCore<RockFsUser>(o =>
        {
            o.Stores.MaxLengthForKeys = 128;
            o.SignIn.RequireConfirmedAccount = true;
        })
            .AddSignInManager()
            .AddRoles<IdentityRole>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = x =>
                {
                    x.Response.Redirect(Urls.UnauthorizedUrl);
                    return Task.CompletedTask;
                },
                OnRedirectToLogout = x =>
                {
                    x.Response.Redirect(Urls.LogoutUrl);
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = x =>
                {
                    x.Response.Redirect(Urls.UnauthorizedUrl);
                    return Task.CompletedTask;
                }
            };

        });
    }
}
