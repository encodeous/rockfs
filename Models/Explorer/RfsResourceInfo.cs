using Humanizer;
using Humanizer.Bytes;

namespace RockFS.Models.Explorer;

public class RfsResourceInfo
{
    public long Size { get; set; }
    public string DisplaySize => ByteSize.FromBytes(Size).Humanize();
    public DateTime LastUpdate { get; set; }
    public string DisplayUpdate => LastUpdate.Humanize();
    public string Extension { get; set; }
}