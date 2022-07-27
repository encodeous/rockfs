using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RockFS.Data;
using RockFS.Models.Auth;
using RockFS.Models.Dashboard;

namespace RockFS.Controllers;

public partial class DashController
{
    private readonly ApplicationDbContext _context;
    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "The user was not found.");
            return View(model);
        }

        var reqUser = await _userManager.GetUserAsync(User);

        var reqUserRole = await _roleService.GetHighestRoleAsync(reqUser);
        var tgtUserRole = await _roleService.GetHighestRoleAsync(user);

        if (user.Id == reqUser.Id)
        {
            if ((int)reqUserRole != (int)model.Role)
            {
                ModelState.AddModelError(string.Empty, "You cannot change your own role.");
                return View(model);
            }
            if (user.Email != model.UserEmail)
            {
                ModelState.AddModelError(string.Empty, "You cannot change your own email.");
                return View(model);
            }
            if (user.EmailConfirmed != model.IsConfirmed)
            {
                ModelState.AddModelError(string.Empty, "You cannot change your own email confirmation status.");
                return View(model);
            }
        }
        else
        {
            if ((int)reqUserRole <= (int)model.Role)
            {
                ModelState.AddModelError(string.Empty, "You do not have permission to grant this role.");
                return View(model);
            }
            if ((int)reqUserRole <= (int)tgtUserRole)
            {
                ModelState.AddModelError(string.Empty, "You do not have permission to modify the information of this user.");
                return View(model);
            }
        }

        if (user.Email != model.UserEmail)
        {
            user.Email = model.UserEmail;
        }
        
        if (user.EmailConfirmed != model.IsConfirmed)
        {
            user.EmailConfirmed = model.IsConfirmed;
        }

        await _userManager.UpdateAsync(user);

        await _roleService.AddUserToRoleAsync(user, model.Role);
        return RedirectToAction("EditUser", "Dash", new { id = model.UserId });
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPost]
    public async Task<IActionResult> AddBlankMount(Guid userId)
    {
        await _rfs.CreateMountAsync(userId);
        return RedirectToAction("EditMounts", "Dash", new { id = userId });
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPost]
    public async Task<IActionResult> EditMounts(EditMountModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId.ToString());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "The user was not found.");
            return View(model);
        }
        foreach (var mount in model.Mounts)
        {
            var mid = new Guid(mount.Key);
            var mnt = await _rfs.GetMountAsync(mid);
            if (mnt is null || mnt.UserId != model.UserId)
            {
                ModelState.AddModelError(string.Empty, $"Invalid mount id \"{mid}\". Please refresh the page.");
                return View(model);
            }
            
            if (mount.Value.MarkForDeletion)
            {
                await _rfs.DeleteMountAsync(mid, false);
            }
            else
            {
                if (!_rfs.ValidateMount(mount.Value.MountPath))
                {
                    ModelState.AddModelError(string.Empty, $"The mount path \"{mount.Value.MountPath}\" was not found.");
                    return View(model);
                }

                mnt.MountPath = _rfs.ProcessMountPath(mount.Value.MountPath);
                mnt.SizeLimit = mount.Value.SizeLimit;
                mnt.MountFriendlyName = mount.Value.MountFriendlyName;

                // sharing options
                if (mnt.IsPublic != mount.Value.IsPublic)
                {
                    if (mount.Value.IsPublic)
                    {
                        mnt.IsPublic = true;
                        mnt.PublicId = Guid.NewGuid();
                    }
                    else
                    {
                        mnt.IsPublic = false;
                        mnt.IsPublicWritable = false;
                        mnt.PublicId = Guid.Empty;
                    }
                }

                if (mnt.IsPublic)
                {
                    mnt.IsPublicWritable = mount.Value.IsPublicWritable;
                }
                else
                {
                    mnt.IsPublicWritable = false;
                }
                
                _context.Mounts.Update(mnt);
            }
        }

        await _context.SaveChangesAsync();
        await _rfs.SaveChangesAsync();
        return RedirectToAction("EditMounts", "Dash", new { id = model.UserId });
    }
}