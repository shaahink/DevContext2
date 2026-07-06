namespace DevContext.Core.Models;

/// <summary>Identifies the logical layer of a project or type within the application architecture.
/// Values cover all D9 archetypes: web/microservices (Api, Application, Domain, Infrastructure, Contracts),
/// library (PublicApi, Core, Internals), and desktop (View, ViewModel, Domain, Platform).</summary>
public enum ArchitectureLayer
{
    Unknown,
    Presentation,
    Api,
    Application,
    Domain,
    Infrastructure,
    Persistence,
    Shared,
    Testing,
    Contracts,
    PublicApi,
    Core,
    Internals,
    View,
    ViewModel,
    Platform,
}

/// <summary>D9 per-archetype layer mapping helpers.</summary>
public static class ArchitectureLayerMapping
{
    public static string ToLabel(this ArchitectureLayer layer, ArchitectureArchetype archetype) =>
        archetype switch
        {
            ArchitectureArchetype.Web or ArchitectureArchetype.Microservices =>
                layer switch
                {
                    ArchitectureLayer.Api => "Api",
                    ArchitectureLayer.Application => "Application",
                    ArchitectureLayer.Domain => "Domain",
                    ArchitectureLayer.Infrastructure => "Infrastructure",
                    ArchitectureLayer.Persistence => "Infrastructure",
                    ArchitectureLayer.Contracts => "Contracts",
                    ArchitectureLayer.Presentation => "Presentation",
                    ArchitectureLayer.Shared => "Shared",
                    _ => layer.ToString(),
                },
            ArchitectureArchetype.Library =>
                layer switch
                {
                    ArchitectureLayer.Api or ArchitectureLayer.Presentation => "PublicApi",
                    ArchitectureLayer.Application => "Core",
                    ArchitectureLayer.Domain => "Core",
                    ArchitectureLayer.Infrastructure => "Internals",
                    ArchitectureLayer.Persistence => "Internals",
                    ArchitectureLayer.Shared => "Core",
                    _ => "Core",
                },
            ArchitectureArchetype.Desktop =>
                layer switch
                {
                    ArchitectureLayer.Api or ArchitectureLayer.Presentation => "View",
                    ArchitectureLayer.Application => "ViewModel",
                    ArchitectureLayer.Domain => "Domain",
                    ArchitectureLayer.Infrastructure => "Platform",
                    ArchitectureLayer.Persistence => "Platform",
                    _ => "ViewModel",
                },
            _ => layer.ToString(),
        };
}

public enum ArchitectureArchetype { Web, Microservices, Library, Desktop, Gateway }

/// <summary>D9 — parses a string (from discovery model or renderer) to the ArchitectureArchetype enum.</summary>
public static class ArchitectureArchetypeParser
{
    public static ArchitectureArchetype Parse(string? archetype) =>
        archetype switch
        {
            "Web" => ArchitectureArchetype.Web,
            "Microservices" => ArchitectureArchetype.Microservices,
            "Library" => ArchitectureArchetype.Library,
            "Desktop" => ArchitectureArchetype.Desktop,
            "Gateway" => ArchitectureArchetype.Gateway,
            _ => ArchitectureArchetype.Web, // default to web-centric mapping
        };
}

/// <summary>D9 — a layer dependency that violates the allowed direction for the architecture style.</summary>
public sealed record LayerViolation(
    string FromNodeId,
    string ToNodeId,
    string FromLayer,
    string ToLayer,
    string EdgeKind,
    string? Provenance);

/// <summary>Identifies the overall architectural style detected from project structure and signals.</summary>
public enum ArchitectureStyle
{
    Unknown,
    CleanArchitecture,
    Onion,
    NLayer,
    VerticalSlices,
    ModularMonolith,
    MinimalApi,
    Microservices,
    ControllerBased
}
