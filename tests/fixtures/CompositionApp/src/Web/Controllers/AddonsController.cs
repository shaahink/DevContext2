using CompositionApp.Core;

using Microsoft.AspNetCore.Mvc;

namespace CompositionApp.Web.Controllers;

/// <summary>
/// Verb attributes carry multi-segment templates (<c>[HttpGet("packs/{id}")]</c>) that compose onto
/// the controller's <c>[Route]</c> prefix. Before the wrap-up fix these collapsed to one duplicate
/// truncated route with a shared wrong target; each action must now render its own composed route.
/// </summary>
[ApiController]
[Route("api/addons")]
public sealed class AddonsController : ControllerBase
{
    private readonly IAddonService _addons;

    public AddonsController(IAddonService addons) => _addons = addons;

    [HttpGet("packs/{id}")]
    public async Task<IActionResult> GetPack(int id)
    {
        var pack = await _addons.GetPackAsync(id);
        return pack is null ? NotFound() : Ok(pack);
    }

    [HttpPost("packs")]
    public async Task<IActionResult> CreatePack(AddonDto dto)
    {
        var created = await _addons.CreatePackAsync(dto.Name);
        return CreatedAtAction(nameof(GetPack), new { id = created.Id }, created);
    }

    [HttpGet("themes/{slug}")]
    public async Task<IActionResult> GetTheme(string slug)
    {
        var theme = await _addons.GetThemeAsync(slug);
        return theme is null ? NotFound() : Ok(theme);
    }
}

public sealed record AddonDto(string Name);
