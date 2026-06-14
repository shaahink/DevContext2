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
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
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
                        && gns.Identifier.ValueText == "DbSet"
                        && gns.TypeArgumentList.Arguments.Count == 1)
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
            if (typeName == "DbContext") return true;

            var baseName = typeName.Split('<')[0];
            if (baseName == "DbContext") return true;
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
                && method.Identifier.ValueText == "OnModelCreating"
                && method.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword))
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

                // Detect ApplyConfigurationsFromAssembly pattern — resolve to actual entity types
                if (method.Body is not null)
                {
                    foreach (var inv in method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (inv.Expression is MemberAccessExpressionSyntax ma
                        && ma.Name.Identifier.ValueText == "ApplyConfigurationsFromAssembly")
                    {
                        var arg = inv.ArgumentList.Arguments.FirstOrDefault()
                            ?.Expression?.ToString() ?? "?";
                        model.AddDiagnostic(DiagnosticLevel.Info, extractorName,
                            $"{dbContextType} uses ApplyConfigurationsFromAssembly({arg}) for entity discovery.");

                        ResolveEntitiesFromConfigurationTypes(model, dbContextType, filePath, lineNumber, extractorName);
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
                && gns.Identifier.ValueText == "DbSet"
                && gns.TypeArgumentList.Arguments.Count == 1
                && gns.TypeArgumentList.Arguments[0].ToString() == entityType);
    }

    private static bool IsAggregateRootPattern(string entityType)
    {
        return entityType.EndsWith("Aggregate")
            || entityType.EndsWith("Root")
            || entityType.Contains("AggregateRoot");
    }

    private static ImmutableArray<string> FindKeyProperties(string entityType)
    {
        if (entityType.Contains("Id")) return [entityType + "Id"];

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
                || method.Identifier.ValueText != "OnModelCreating"
                || method.Body == null)
                continue;

            var lineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            foreach (var inv in method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                ct.ThrowIfCancellationRequested();

                if (inv.Expression is not MemberAccessExpressionSyntax ma) continue;

                var methodName = ma.Name.Identifier.ValueText;

                // modelBuilder.Entity<T>() or builder.Entity<T>()
                if ((methodName == "Entity"
                     || methodName == "RegisterAllDerivedEntities")
                    && inv.Expression is MemberAccessExpressionSyntax ma2
                    && (ma2.Expression.ToString().Contains("modelBuilder")
                        || ma2.Expression.ToString().Contains("builder")))
                {
                    // Try generic type argument or string argument
                    if (ma.Name is GenericNameSyntax gns
                        && gns.TypeArgumentList.Arguments.Count > 0)
                    {
                        var entityTypeName = gns.TypeArgumentList.Arguments[0].ToString();
                        if (entityTypeName.Length < 2 || entityTypeName[0] is 'T' or 't') continue;

                        if (methodName == "RegisterAllDerivedEntities")
                        {
                            ResolveEntitiesDerivedFrom(entityTypeName, model, dbContextType, filePath, lineNumber);
                        }
                        else
                        {
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
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
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
                    && classDecl.BaseList.Types.Any(t => t.Type.ToString().Contains("Migration")))
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

    /// <summary>Resolves entity types from IEntityTypeConfiguration&lt;T&gt; implementations in the model.</summary>
    private static void ResolveEntitiesFromConfigurationTypes(
        DiscoveryModel model, string dbContextType, string filePath, int lineNumber, string extractorName)
    {
        foreach (var type in model.Types.Values)
        {
            foreach (var iface in type.ImplementedInterfaces)
            {
                // Match IEntityTypeConfiguration<T> or IEntityTypeConfiguration<T, ...>
                var name = iface.Split('<')[0].Trim();
                if (!name.EndsWith("IEntityTypeConfiguration", StringComparison.Ordinal)) continue;

                // Extract the first generic argument as the entity type
                var start = iface.IndexOf('<');
                var end = iface.LastIndexOf('>');
                if (start < 0 || end <= start) continue;

                var args = iface[(start + 1)..end];
                var firstArg = args.Split(',')[0].Trim();

                if (firstArg.Length < 2 || firstArg is "T" or "TEntity") continue;

                var keyProps = FindKeyProperties(firstArg);
                model.Detections.Add(new EfEntityDetection(
                    EntityType: firstArg,
                    DbContextType: dbContextType,
                    IsAggregate: IsAggregateRootPattern(firstArg),
                    KeyProperties: keyProps)
                {
                    ExtractorName = extractorName,
                    SourceFile = type.FilePath,
                    LineNumber = lineNumber,
                    Confidence = 0.85f,
                });
            }
        }
    }

    /// <summary>Resolves entity types from all concrete types deriving from the given base type.</summary>
    private static void ResolveEntitiesDerivedFrom(
        string baseTypeName, DiscoveryModel model, string dbContextType, string filePath, int lineNumber)
    {
        foreach (var type in model.Types.Values)
        {
            if (type.Kind is DevContext.Core.Models.TypeKind.Interface or DevContext.Core.Models.TypeKind.Enum or DevContext.Core.Models.TypeKind.Delegate) continue;

            // Check if the type's base types include the target, or its namespace is a sub-namespace
            var derivesFrom = type.BaseTypes.Any(b =>
                b == baseTypeName || b.Contains("." + baseTypeName, StringComparison.Ordinal));

            if (!derivesFrom) continue;

            var keyProps = FindKeyProperties(type.Name);
            model.Detections.Add(new EfEntityDetection(
                EntityType: type.Name,
                DbContextType: dbContextType,
                IsAggregate: IsAggregateRootPattern(type.Name),
                KeyProperties: keyProps)
            {
                ExtractorName = "EfCoreExtractor",
                SourceFile = type.FilePath,
                LineNumber = lineNumber,
                Confidence = 0.75f,
            });
        }
    }
}
