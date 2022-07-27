using RockFS.Data.Models;

namespace RockFS.Models.Dashboard;

public class EditMountModel
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; }
    public Dictionary<string, MountEntry> Mounts { get; set; }
}