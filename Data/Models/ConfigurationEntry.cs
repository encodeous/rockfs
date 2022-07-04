using System.ComponentModel.DataAnnotations;

namespace RockFS.Data.Models;

public class ConfigurationEntry
{
    [Key]
    public string? Key { get; set; }

    public string? Value { get; set; }
}