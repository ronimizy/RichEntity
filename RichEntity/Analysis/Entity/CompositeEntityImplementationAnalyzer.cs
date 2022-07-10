using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RichEntity.Extensions;
using RichEntity.Utility;

namespace RichEntity.Analysis.Entity;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CompositeEntityImplementationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RE2002";
    public const string Title = nameof(KeyEntityTypeAnalyzer);

    public const string Format = "Type (\"{0}\") must implement an \"IEntity\" interface to contain [KeyEntity] properties";

    private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
        DiagnosticId, Title, Format, "EntityGeneration", DiagnosticSeverity.Error, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Descriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        
        context.RegisterCompilationStartAction(c =>
        {
            var compositeEntityInterface = c.Compilation
                .GetTypeByMetadataName(Constants.CompositeEntityInterfaceFullyQualifiedName);

            var keyEntityAttribute = c.Compilation
                .GetTypeByMetadataName(Constants.KeyEntityAttributeFullyQualifiedName);
            
            if (compositeEntityInterface is null || keyEntityAttribute is null)
                return;
            
            c.RegisterSymbolAction(AnalyzeTypeSymbol, SymbolKind.NamedType);
        });
    }

    private static void AnalyzeTypeSymbol(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
        
        var compositeEntityInterface = context.Compilation
            .GetTypeByMetadataName(Constants.CompositeEntityInterfaceFullyQualifiedName)!;

        var keyEntityAttribute = context.Compilation
            .GetTypeByMetadataName(Constants.KeyEntityAttributeFullyQualifiedName)!;
        
        if (typeSymbol.IsAssignableTo(compositeEntityInterface))
            return;
        
        if (!typeSymbol.GetMembers().Any(m => m.HasAttribute(keyEntityAttribute)))
            return;
        
        var diagnostic = Diagnostic.Create(Descriptor, typeSymbol.Locations[0], typeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}