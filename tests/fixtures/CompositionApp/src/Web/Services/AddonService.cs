using CompositionApp.Core;

namespace CompositionApp.Web.Services;

public sealed class AddonService : IAddonService
{
    public Task<Addon?> GetPackAsync(int id) => Task.FromResult<Addon?>(new Addon(id, $"pack-{id}"));

    public Task<Addon> CreatePackAsync(string name) => Task.FromResult(new Addon(1, name));

    public Task<Addon?> GetThemeAsync(string slug) => Task.FromResult<Addon?>(new Addon(0, slug));
}
