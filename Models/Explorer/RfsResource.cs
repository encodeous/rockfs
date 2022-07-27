using System.Text.Json.Serialization;

namespace RockFS.Models.Explorer;

public class RfsResource
{
    public static readonly RfsResource NoAccess = new ()
    {
        CanRead = false,
        CanWrite = false
    };
    public bool CanRead { get; init; }
    public bool CanWrite { get; init; }
    public bool IsDirectory { get; init; }
    public string? RelativePath { get; init; }
    [JsonIgnore]
    public string? AbsolutePath { get; init; }
    public string? Name { get; init; }
}