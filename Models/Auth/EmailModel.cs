using System.ComponentModel.DataAnnotations;

namespace RockFS.Models.Auth;

public class EmailModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }
}