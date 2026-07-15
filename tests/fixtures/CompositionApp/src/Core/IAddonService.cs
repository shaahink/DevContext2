namespace CompositionApp.Core;

public interface IAddonService
{
    Task<Addon?> GetPackAsync(int id);
    Task<Addon> CreatePackAsync(string name);
    Task<Addon?> GetThemeAsync(string slug);
}
