namespace DevContext.Core.Models;

/// <summary>Identifies the logical layer of a project or type within the application architecture.</summary>
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
    Testing
}

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
    Microservices
}
