using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Humanizer;
using Humanizer.Bytes;

namespace RockFS.Data.Models;

public class MountEntry
{
    [Key]
    public Guid MountId { get; set; }
    public Guid UserId { get; set; }
    public string MountFriendlyName { get; set; }
    public string MountPath { get; set; }
    public long SizeLimit { get; set; }
    public Guid PublicId { get; set; }
    public bool IsPublic { get; set; }
    public bool IsPublicWritable { get; set; }
    /// <summary>
    /// Property used for model binding
    /// </summary>
    [NotMapped]
    public string FormattedSizeLimit {
        get
        {
            if (SizeLimit == -1) return "-1";
            return ByteSize.FromBytes(SizeLimit).Humanize();
        }
        set
        {
            if (value == "-1")
            {
                SizeLimit = -1;
            }
            else
            {
                var formatted = ByteSize.FromBytes(SizeLimit).Humanize();
                if (formatted != value)
                {
                    SizeLimit = (long)ByteSize.Parse(value).Bytes;
                }
            }
        }
    }
    /// <summary>
    /// Property used for model binding
    /// </summary>
    [NotMapped]
    public bool MarkForDeletion { get; set; }
}