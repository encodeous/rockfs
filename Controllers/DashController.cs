using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RockFS.Data;
using RockFS.Data.Models;
using RockFS.Models.Auth;
using RockFS.Models.Dashboard;
using RockFS.Models.Explorer;
using RockFS.Services.RockFS;
using RockFS.Views.Explorer;

namespace RockFS.Controllers;

[Authorize(Roles = nameof(UserRole.Member))]
public partial class DashController : Controller
{
    private readonly UserManager<RockFsUser> _userManager;
    private readonly RoleService _roleService;
    private readonly RfsService _rfs;

    public DashController(UserManager<RockFsUser> userManager, RoleService roleService, RfsService rfs, ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleService = roleService;
        _rfs = rfs;
        _context = context;
    }

    public IActionResult Mounts()
    {
        return View();
    }
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public IActionResult Users()
    {
        return View();
    }
    [Authorize(Roles = nameof(UserRole.Member))]
    [Route("dash/explore/{mountId:guid}/{*path}")]
    public async Task<IActionResult> Explore(Guid mountId, string? path)
    {
        if (path == null || path == "/") path = "";
        var user = await _userManager.GetUserAsync(User);
        var grant = await _rfs.GetGrant(mountId, new Guid(user.Id));
        string name = "Unauthorized Mount";
        if (grant.MountId != Guid.Empty)
        {
            var mount = await _rfs.GetMountAsync(grant.MountId);
            name = mount.MountFriendlyName;
        }
        var resources = await _rfs.GetResources(grant, path);
        return View(new MountExplorerModel(grant, path, name, resources));
    }
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public async Task<IActionResult> EditMounts(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return RedirectToAction("NotFound", "Home");
        return View(new EditMountModel()
        {
            UserId = new Guid(id),
            UserEmail = user.Email,
            Mounts = (from x in await _rfs.GetUserMountsAsync(new Guid(id))
                select (x.MountId.ToString(), x)).ToDictionary(x=>x.Item1, x=>x.x)
        });
    }
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return RedirectToAction("NotFound", "Home");
        return View(new EditUserModel()
        {
            Role = await _roleService.GetHighestRoleAsync(user),
            IsConfirmed = user.EmailConfirmed,
            UserId = id,
            UserEmail = user.Email,
        });
    }
}