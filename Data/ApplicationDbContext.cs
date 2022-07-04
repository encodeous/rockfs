using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RockFS.Data.Models;

namespace RockFS.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<ConfigurationEntry> ConfigSet { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}