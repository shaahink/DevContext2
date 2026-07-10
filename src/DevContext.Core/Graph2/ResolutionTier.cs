namespace DevContext.Core.Graph2;

public enum ResolutionTier
{
    Declared,
    Semantic,
    FileScoped,
    ProjectScoped,
    GlobalUnique,
    Ambiguous,
    Unresolved,
}
