using System.ComponentModel.DataAnnotations;
using RockFS.Models.Auth;

namespace RockFS.Models.Dashboard;

public class EditUserModel
{
    [Display(Name = "User Identifier")]
    public string UserId { get; set; }
    [Display(Name = "User Email")]
    [EmailAddress]
    public string UserEmail { get; set; }
    [Display(Name = "Has Confirmed Email")]
    public bool IsConfirmed { get; set; }
    [Display(Name = "User Role")]
    public UserRole Role { get; set; }
}