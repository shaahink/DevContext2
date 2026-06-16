using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects EF Core DbContext types, DbSet properties, and entity configurations via syntax tree analysis.</summary>
[ExtractorOrder(30)]
public sealed class EfCoreExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "EfCoreExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.EfCore], ["ef-entity-detections"],
        ["model.Detections"],
        "Walks syntax trees to detect EF Core DbContext, DbSet properties, and entity configurations");
    /// <summary>Only runs when the EF Core signal has been detected.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.EfCore);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classes)
            {
                ct.ThrowIfCancellationRequested();

                if (!DerivesFromDbContext(classDecl)) continue;

                var dbContextType = classDecl.Identifier.ValueText;
                var lineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                var dbSetProperties = classDecl.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(p => p.Type is GenericNameSyntax gns
                        && string.Equals(gns.Identifier.ValueText, "DbSet"
, StringComparison.Ordinal) && gns.TypeArgumentList.Arguments.Count == 1)
                    .ToList();

                foreach (var dbSetProp in dbSetProperties)
                {
                    var entityType = ((GenericNameSyntax)dbSetProp.Type).TypeArgumentList.Arguments[0].ToString();
                    var isAggregate = HasOwnDbSet(entityType, classDecl) || IsAggregateRootPattern(entityType);
                    var keyProps = FindKeyProperties(entityType);

                    model.Detections.Add(new EfEntityDetection(
                        EntityType: entityType,
                        DbContextType: dbContextType,
                        IsAggregate: isAggregate,
                        KeyProperties: keyProps)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }

                DetectOnModelCreatingOverrides(classDecl, filePath, dbContextType, model, Name, ct, context);

                // Detect entities via modelBuilder.Entity<T>() in OnModelCreating
                DetectEntitiesFromOnModelCreating(classDecl, filePath, dbContextType, model, context, ct);
            }
        }

        await DetectMigrationsFolder(context, model, Name, ct).ConfigureAwait(false);
    }

    private static bool DerivesFromDbContext(ClassDeclarationSyntax classDecl)
    {
        if (classDecl.BaseList == null) return false;

        foreach (var baseType in classDecl.BaseList.Types)
        {
            var typeName = baseType.Type.ToString();
            if (string.Equals(typeName, "DbContext", StringComparison.Ordinal)) return true;

            var baseName = typeName.Split('<')[0];
            if (string.Equals(baseName, "DbContext", StringComparison.Ordinal)) return true;
        }

        return false;
    }

    private static void DetectOnModelCreatingOverrides(
        ClassDeclarationSyntax classDecl,
        string filePath,
        string dbContextType,
        DiscoveryModel model,
        string extractorName,
        CancellationToken ct,
        DiscoveryContext context)
    {
        ct.ThrowIfCancellationRequested();

        foreach (var member in classDecl.Members)
        {
            if (member is MethodDeclarationSyntax method
                && string.Equals(method.Identifier.ValueText, "OnModelCreating"
, StringComparison.Ordinal) && method.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword))
                && method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
            {
                var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                model.Detections.Add(new EfEntityDetection(
                    EntityType: "<OnModelCreating>",
                    DbContextType: dbContextType,
                    IsAggregate: false,
                    KeyProperties: [])
                {
                    ExtractorName = extractorName,
                    SourceFile = filePath,
                    LineNumber = lineNumber,
                    Confidence = 0.8f,
                });

                // Detect ApplyConfigurationsFromAssembly pattern
                if (method.Body is not null)
                {
                    foreach (var inv in method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (inv.Expression is MemberAccessExpressionSyntax ma
                        && string.Equals(ma.Name.Identifier.ValueText, "ApplyConfigurationsFromAssembly", StringComparison.Ordinal))
                    {
                        var arg = inv.ArgumentList.Arguments.FirstOrDefault()
                            ?.Expression?.ToString() ?? "?";
                        model.AddDiagnostic(DiagnosticLevel.Info, extractorName,
                            $"{dbContextType} uses ApplyConfigurationsFromAssembly({arg}) for entity discovery.");
                    }
                }
            }
        }
    }
    }

    private static bool HasOwnDbSet(string entityType, ClassDeclarationSyntax dbContextClass)
    {
        return dbContextClass.Members
            .OfType<PropertyDeclarationSyntax>()
            .Any(p => p.Type is GenericNameSyntax gns
                && string.Equals(gns.Identifier.ValueText, "DbSet"
, StringComparison.Ordinal) && gns.TypeArgumentList.Arguments.Count == 1
                && string.Equals(gns.TypeArgumentList.Arguments[0].ToString(), entityType, StringComparison.Ordinal));
    }

    private static bool IsAggregateRootPattern(string entityType)
    {
        return entityType.EndsWith("Aggregate", StringComparison.Ordinal)
            || entityType.EndsWith("Root", StringComparison.Ordinal)
            || entityType.Contains("AggregateRoot", StringComparison.Ordinal);
    }

    private static ImmutableArray<string> FindKeyProperties(string entityType)
    {
        if (entityType.Contains("Id", StringComparison.Ordinal)) return [entityType + "Id"];

        return ["Id"];
    }

    private static void DetectEntitiesFromOnModelCreating(
        ClassDeclarationSyntax classDecl,
        string filePath,
        string dbContextType,
        DiscoveryModel model,
        DiscoveryContext context,
        CancellationToken ct)
    {
        foreach (var member in classDecl.Members)
        {
            if (member is not MethodDeclarationSyntax method
                || !string.Equals(method.Identifier.ValueText, "OnModelCreating"
, StringComparison.Ordinal) || method.Body == null)
                continue;

            var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            foreach (var inv in method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                ct.ThrowIfCancellationRequested();

                if (inv.Expression is not MemberAccessExpressionSyntax ma) continue;

                var methodName = ma.Name.Identifier.ValueText;

                // modelBuilder.Entity<T>() or builder.Entity<T>()
                if ((string.Equals(methodName, "Entity"
, StringComparison.Ordinal) || string.Equals(methodName, "RegisterAllDerivedEntities", StringComparison.Ordinal))
                    && inv.Expression is MemberAccessExpressionSyntax ma2
                    && (ma2.Expression.ToString().Contains("modelBuilder", StringComparison.Ordinal)
                        || ma2.Expression.ToString().Contains("builder", StringComparison.Ordinal)))
                {
                    // Try generic type argument or string argument
                    if (ma.Name is GenericNameSyntax gns
                        && gns.TypeArgumentList.Arguments.Count > 0)
                    {
                        var entityTypeName = gns.TypeArgumentList.Arguments[0].ToString();
                        // Skip if it's a generic parameter from the enclosing method
                        if (entityTypeName.Length < 2 || entityTypeName[0] is 'T' or 't') continue;

                        var keyProps = FindKeyProperties(entityTypeName);
                        model.Detections.Add(new EfEntityDetection(
                            EntityType: entityTypeName,
                            DbContextType: dbContextType,
                            IsAggregate: IsAggregateRootPattern(entityTypeName),
                            KeyProperties: keyProps)
                        {
                            ExtractorName = "EfCoreExtractor",
                            SourceFile = filePath,
                            LineNumber = lineNumber,
                            Confidence = 0.7f,
                        });
                    }
                    else if (inv.ArgumentList.Arguments.Count > 0)
                    {
                        var arg = inv.ArgumentList.Arguments[0].Expression;
                        if (arg is TypeOfExpressionSyntax tof)
                        {
                            var entityTypeName = tof.Type.ToString();
                            var keyProps = FindKeyProperties(entityTypeName);
                            model.Detections.Add(new EfEntityDetection(
                                EntityType: entityTypeName,
                                DbContextType: dbContextType,
                                IsAggregate: IsAggregateRootPattern(entityTypeName),
                                KeyProperties: keyProps)
                            {
                                ExtractorName = "EfCoreExtractor",
                                SourceFile = filePath,
                                LineNumber = lineNumber,
                                Confidence = 0.7f,
                            });
                        }
                    }
                }
            }
        }
    }

    private static async ValueTask DetectMigrationsFolder(
        DiscoveryContext context,
        DiscoveryModel model,
        string extractorName,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();

            if (!filePath.Contains("Migrations", StringComparison.OrdinalIgnoreCase)
                || !filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
            }
            catch
            {
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classes)
            {
                if (classDecl.BaseList != null
                    && classDecl.BaseList.Types.Any(t => t.Type.ToString().Contains("Migration", StringComparison.Ordinal)))
                {
                    var lineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    model.Detections.Add(new EfEntityDetection(
                        EntityType: classDecl.Identifier.ValueText,
                        DbContextType: "Migrations",
                        IsAggregate: false,
                        KeyProperties: [])
                    {
                        ExtractorName = extractorName,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                        Confidence = 0.9f,
                    });
                }
            }
        }
    }
}
