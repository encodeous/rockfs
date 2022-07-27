using Newtonsoft.Json;

namespace RockFS;

public class Urls
{
    [JsonProperty] public const string AuthorizedUrl = "/authorized";
    public const string MountSettingsUrl = "/dash/settings";
    public const string ExplorerUrl = "/dash/explore";
    public const string EditUserUrl = "/dash/edituser";
    public const string EditMountsUrl = "/dash/editmounts";
    public const string UsersUrl = "/dash/users";
    [JsonProperty] public const string IndexUrl = "/";
    public const string ErrorUrl = "/error";
    public const string LoginUrl = "/auth/login";
    public const string RegisterUrl = "/auth/register";
    public const string ForgotPasswordUrl = "/auth/forgotpassword";
    public const string ResendEmailUrl = "/auth/resendconfirmemail";
    public const string LogoutUrl = "/auth/logout";
    public const string ConfirmUrl = $"/auth/confirmemail";
    public const string ResetPasswordUrl = $"/auth/resetpassword";
    [JsonProperty] public const string PrivacyUrl = "/privacy";
    [JsonProperty] public const string SpaUrl = "/spa";
    public const string UnauthorizedUrl = "/auth/accessdenied";
}