using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RichEntity.Extensions;
using RichEntity.Utility;

namespace RichEntity.Analysis.Entity;

// [DiagnosticAnalyzer(LanguageNames.CSharp)]
public class KeyEntityTypeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RE2001";
    public const string Title = nameof(KeyEntityTypeAnalyzer);
    public const string Format = "Property's type (\"{0}\") must implement an \"IEntity<>\" interface to be a KeyEntity type";

    public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
        DiagnosticId, Title, Format, "EntityGeneration", DiagnosticSeverity.Error, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Descriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                               GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(c =>
        {
            var keyEntityAttributeTypeSymbol = c.Compilation
                .GetTypeByMetadataName(Constants.KeyEntityAttributeFullyQualifiedName);

            if (keyEntityAttributeTypeSymbol is null)
                return;

            var entityInterfaceTypeSymbol = c.Compilation
                .GetTypeByMetadataName(Constants.EntityInterfaceFullyQualifiedName);

            if (entityInterfaceTypeSymbol is null)
                return;

            c.RegisterSymbolAction(AnalyzePropertySymbol, SymbolKind.Property);
        });
    }

    private static void AnalyzePropertySymbol(SymbolAnalysisContext context)
    {
        var propertySymbol = (IPropertySymbol)context.Symbol;

        var keyEntityAttribute = context.Compilation
            .GetTypeByMetadataName(Constants.KeyEntityAttributeFullyQualifiedName)!;

        if (!propertySymbol.HasAttribute(keyEntityAttribute))
            return;

        var entityInterfaceTypeSymbol = context.Compilation
            .GetTypeByMetadataName(Constants.EntityInterfaceFullyQualifiedName)!;

        if (propertySymbol.Type.IsAssignableTo(entityInterfaceTypeSymbol))
            return;

        var diagnostic = Diagnostic.Create(Descriptor, propertySymbol.Locations[0],
            propertySymbol.Type.GetFullyQualifiedName());
        
        context.ReportDiagnostic(diagnostic);
    }
}