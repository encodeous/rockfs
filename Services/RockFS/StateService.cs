using RockFS.Configuration;
using RockFS.Data;

namespace RockFS.Services.RockFS;

public class StateService
{
    private readonly ApplicationDbContext _context;

    public StateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> RequiresSetup()
    {
        var res = await _context.GetConfigAsync<bool>(ConfigManager.ConfigKeys.IsSetupFinished);
        return res is null or false;
    }

    public async Task FinishSetup()
    {
        await _context.SetConfigAsync(ConfigManager.ConfigKeys.IsSetupFinished, true);
        await _context.SaveChangesAsync();
    }
    
}