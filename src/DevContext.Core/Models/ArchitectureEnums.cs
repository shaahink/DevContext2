namespace DevContext.Core.Models;

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
