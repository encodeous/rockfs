using System.ComponentModel.DataAnnotations;

namespace RockFS.Services.Email;

public class AuthMessageSenderOptions
{
    public string? SendGridKey { get; set; }
    [EmailAddress]
    public string EmailAddress { get; set; }
}