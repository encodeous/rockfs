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

    public RfsResourceInfo? Info
    {
        get
        {
            if (!CanRead) return null;
            long size = 0;
            string extension = "";
            if (!IsDirectory)
            {
                var fi = new FileInfo(AbsolutePath);
                size = fi.Length;
                extension = fi.Extension.TrimStart('.');
            }
            return new RfsResourceInfo()
            {
                LastUpdate = File.GetLastWriteTimeUtc(AbsolutePath),
                Size = size,
                Extension = extension
            };
        }
    }
}