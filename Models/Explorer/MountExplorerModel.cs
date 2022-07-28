using RockFS.Models.Explorer;

namespace RockFS.Views.Explorer;

public class MountExplorerModel
{
    public MountExplorerModel(RfsGrant grant, string folder, string mountName, RfsResource[] resources)
    {
        Grant = grant;
        Folder = folder;
        MountName = mountName;
        Resources = resources;
    }

    public RfsGrant Grant { get; set; }
    public string Folder { get; set;}
    public string MountName { get;  set; }
    public string BasePath { get;  set; }
    public string CurPath { get;  set; }
    public RfsResource[] Resources { get;  set; }
}