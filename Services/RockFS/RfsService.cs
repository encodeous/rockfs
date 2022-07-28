using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RockFS.Data;
using RockFS.Data.Models;
using RockFS.Models.Auth;
using RockFS.Models.Explorer;

namespace RockFS.Services.RockFS;

public class RfsService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<RockFsUser> _userManager;
    private readonly RoleService _roleService;

    public RfsService(ApplicationDbContext context, UserManager<RockFsUser> userManager, RoleService roleService)
    {
        _context = context;
        _userManager = userManager;
        _roleService = roleService;
    }

    public bool ValidateMount(string mount)
    {
        return Directory.Exists(mount);
    }

    public string ProcessMountPath(string source)
    {
        var dir = new DirectoryInfo(source).FullName;
        return Path.TrimEndingDirectorySeparator(dir);
    }

    public async Task<MountEntry?> GetMountAsync(Guid mountId)
    {
        return await _context.Mounts.FindAsync(mountId);
    }

    public async Task<List<MountEntry>> GetUserMountsAsync(Guid userId)
    {
        return await (from x in _context.Mounts
            where x.UserId == userId
            select x).ToListAsync();
    }

    public async Task<MountEntry> CreateMountAsync(Guid userId, bool save = true)
    {
        var entry = new MountEntry()
        {
            MountId = Guid.NewGuid(),
            MountPath = "RFS-Undefined-Path",
            UserId = userId,
            SizeLimit = -1,
            MountFriendlyName = "RFS-Undefined-Name"
        };
        await _context.Mounts.AddAsync(entry);
        if(save) await _context.SaveChangesAsync();
        return entry;
    }

    public async Task DeleteMountAsync(Guid mountId, bool save = true)
    {
        var mount = await GetMountAsync(mountId);
        if (mount is null) return;
        _context.Mounts.Remove(mount);
        if(save) await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<long> GetMountUsedSize(Guid mountId)
    {
        long size = 0;
        Stack<DirectoryInfo> diStack = new();
        diStack.Push(new DirectoryInfo((await GetMountAsync(mountId))!.MountPath));
        var st = DateTime.UtcNow;
        while (diStack.Any())
        {
            var top = diStack.Pop();
            if (DateTime.UtcNow - st > TimeSpan.FromMilliseconds(100)) return long.MaxValue;
            try
            {
                foreach (var dir in top.EnumerateDirectories())
                {
                    if (dir.Attributes.HasFlag(FileAttributes.System) ||
                        dir.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        continue;
                    }
                    diStack.Push(dir);
                }
                foreach (var file in top.EnumerateFiles())
                {
                    if (file.Attributes.HasFlag(FileAttributes.System) ||
                        file.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        continue;
                    }
                    size += file.Length;
                }
            }
            catch
            {
                // ignored
            }
        }
        return size;
    }
    
    /// <summary>
    /// Gets and validates a grant to a file system mount
    /// </summary>
    /// <param name="objectId">Either a mount id or a share id</param>
    /// <param name="authUserId">if the object id is a mount id, then the auth user id is required</param>
    /// <returns></returns>
    public Task<RfsGrant> GetGrant(Guid objectId, Guid? authUserId = default)
    {
        if (authUserId is null)
        {
            return GetGrantFromShareId(objectId);
        }

        return GetGrantFromUser(authUserId.Value, objectId);
    }

    private async Task<RfsGrant> GetGrantFromShareId(Guid shareId)
    {
        var mount = await (from x in _context.Mounts
            where x.PublicId == shareId
            select x).FirstOrDefaultAsync();
        if (mount is null)
        {
            return RfsGrant.None;
        }
        else
        {
            return new RfsGrant()
            {
                MountId = mount.MountId,
                CanManage = false,
                CanWrite = mount.IsPublicWritable
            };
        }
    }

    private async Task<RfsGrant> GetGrantFromUser(Guid userId, Guid mountId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var mount = await (from x in _context.Mounts
            where x.MountId == mountId
            select x).FirstOrDefaultAsync();
        if (user is null || mount is null)
        {
            return RfsGrant.None;
        }

        // Getting the grant directly by the user id does not give permissions allotted by the public share link.
        if ((int)(await _roleService.GetHighestRoleAsync(user)) >= (int)UserRole.Administrator
            || userId == mount.UserId)
        {
            return new RfsGrant()
            {
                MountId = mount.MountId,
                CanManage = true,
                CanWrite = true
            };
        }

        return RfsGrant.None;
    }

    public async Task<RfsResource[]> GetResources(RfsGrant grant, string folderPath)
    {
        var res = await GetResource(grant, folderPath);
        if (!res.CanRead || !res.IsDirectory)
        {
            return Array.Empty<RfsResource>();
        }

        var di = new DirectoryInfo(res.AbsolutePath);
        var lst = new List<RfsResource>();
        foreach (var dir in di.EnumerateDirectories())
        {
            if (dir.Attributes.HasFlag(FileAttributes.System) ||
                dir.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                continue;
            }
            lst.Add(new RfsResource()
            {
                AbsolutePath = dir.FullName,
                CanRead = true,
                CanWrite = grant.CanWrite,
                IsDirectory = true,
                RelativePath = Path.GetRelativePath(di.FullName, dir.FullName),
                Name = dir.Name
            });
        }
        foreach (var file in di.EnumerateFiles())
        {
            if (file.Attributes.HasFlag(FileAttributes.System) ||
                file.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                continue;
            }
            lst.Add(new RfsResource()
            {
                AbsolutePath = file.FullName,
                CanRead = true,
                CanWrite = grant.CanWrite,
                IsDirectory = false,
                RelativePath = Path.GetRelativePath(di.FullName, file.FullName),
                Name = Path.GetFileName(file.FullName)
            });
        }

        return lst.ToArray();
    }

    public async Task<RfsResource> GetResource(RfsGrant grant, string path)
    {
        if (path == "") path = Path.DirectorySeparatorChar + "";
        if (grant.MountId == Guid.Empty)
        {
            return RfsResource.NoAccess;
        }
        var mount = await GetMountAsync(grant.MountId);
        var combinedPath = Path.Join(mount.MountPath, path);
        if (Path.EndsInDirectorySeparator(combinedPath))
        {
            if (Directory.Exists(combinedPath))
            {
                // just a security check
                if (!combinedPath.StartsWith(mount.MountPath))
                {
                    return RfsResource.NoAccess;
                }

                return new RfsResource()
                {
                    AbsolutePath = combinedPath,
                    CanRead = true,
                    CanWrite = grant.CanWrite,
                    IsDirectory = true,
                    RelativePath = Path.GetRelativePath(mount.MountPath, combinedPath),
                    Name = Path.GetDirectoryName(combinedPath)
                };
            }
        }
        else
        {
            if (File.Exists(combinedPath))
            {
                // just a security check
                if (!combinedPath.StartsWith(mount.MountPath))
                {
                    return RfsResource.NoAccess;
                }

                return new RfsResource()
                {
                    AbsolutePath = combinedPath,
                    CanRead = true,
                    CanWrite = grant.CanWrite,
                    IsDirectory = false,
                    RelativePath = Path.GetRelativePath(mount.MountPath, combinedPath),
                    Name = Path.GetFileName(combinedPath)
                };
            }
        }

        return RfsResource.NoAccess;
    }
}