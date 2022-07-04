using RockFS.Data;
using RockFS.Data.Models;
using System.Text.Json;

namespace RockFS.Configuration;

public static class ConfigManager
{
    public enum ConfigKeys
    {
        SetupStage,
        IsSetupFinished
    }

    public static async Task SetConfigAsync(this ApplicationDbContext context, ConfigKeys key, string value)
    {
        var res = await context.ConfigSet.FindAsync(key);
        if (res is not null)
        {
            context.ConfigSet.Remove(res);
        }

        await context.ConfigSet.AddAsync(new ConfigurationEntry()
        {
            Key = key.ToString(),
            Value = value
        });
    }
    
    public static async Task SetConfigAsync<T>(this ApplicationDbContext context, ConfigKeys key, T value)
    {
        var res = await context.ConfigSet.FindAsync(key.ToString());
        if (res is not null)
        {
            context.ConfigSet.Remove(res);
        }

        await context.ConfigSet.AddAsync(new ConfigurationEntry()
        {
            Key = key.ToString(),
            Value = JsonSerializer.Serialize(value)
        });
    }

    public static async ValueTask<string?> GetConfigAsync(this ApplicationDbContext context, ConfigKeys key)
    {
        return (await context.ConfigSet.FindAsync(key.ToString()))?.Value;
    }

    public static async Task<T?> GetConfigObjectAsync<T>(this ApplicationDbContext context, ConfigKeys key) where T : class
    {
        var res = await context.ConfigSet.FindAsync(key.ToString());
        if (res is null) return default;
        return JsonSerializer.Deserialize<T>(res.Value);
    }
    public static async Task<T?> GetConfigAsync<T>(this ApplicationDbContext context, ConfigKeys key) where T : struct
    {
        var res = await context.ConfigSet.FindAsync(key.ToString());
        if (res is null) return default;
        return JsonSerializer.Deserialize<T>(res.Value);
    }
}
