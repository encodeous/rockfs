using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RockFS.Data.Models;

namespace RockFS.Data;

public class ApplicationDbContext : IdentityDbContext<RockFsUser>, IDataProtectionKeyContext
{
    public DbSet<ConfigurationEntry> ConfigSet { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<MountEntry> Mounts { get; set; }
}