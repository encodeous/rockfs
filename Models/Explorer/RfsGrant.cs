namespace RockFS.Models.Explorer;

public class RfsGrant
{
    public static readonly RfsGrant None = new ()
    {
        MountId = Guid.Empty
    };
    public Guid MountId { get; init; }
    public bool CanWrite { get; init; }
    public bool CanManage { get; init; }
}