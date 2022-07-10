using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RichEntity.Extensions;
using RichEntity.Utility;

namespace RichEntity.Analysis.Entity;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PartialTypeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RE2000";
    public const string Title = "TypeMustBePartial";
    public const string Format = "Type \"{0}\" must be partial to be a generated entity";

    public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
        DiagnosticId, Title, Format, "EntityGeneration", DiagnosticSeverity.Error, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Descriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                               GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(ctx =>
        {
            var entityInterface = ctx.Compilation
                .GetTypeByMetadataName(Constants.EntityInterfaceFullyQualifiedName);
            
            var compositeEntityInterface = ctx.Compilation
                .GetTypeByMetadataName(Constants.CompositeEntityInterfaceFullyQualifiedName);
            
            if (entityInterface is null || compositeEntityInterface is null)
                return;

            ctx.RegisterSyntaxNodeAction(ProcessNode,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.RecordStructDeclaration);
        });
    }

    private static void ProcessNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax syntax)
            return;

        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not INamedTypeSymbol typeSymbol)
            return;

        var entityInterfaceSymbol = context.Compilation
            .GetTypeByMetadataName(Constants.EntityInterfaceFullyQualifiedName)!;
        
        var compositeEntityInterfaceSymbol = context.Compilation
            .GetTypeByMetadataName(Constants.CompositeEntityInterfaceFullyQualifiedName)!;

        if (!typeSymbol.AllInterfaces.Any(i => i.IsAssignableTo(entityInterfaceSymbol)) &&
            !typeSymbol.AllInterfaces.Any(i => i.IsAssignableTo(compositeEntityInterfaceSymbol)))
            return;

        if (Conforming(syntax))
            return;
        
        context.ReportDiagnostic(Create(syntax, typeSymbol));
    }

    public static Diagnostic Create(TypeDeclarationSyntax syntax, INamedTypeSymbol symbol)
    {
        var location = syntax.Identifier.GetLocation();
        var typeName = symbol.GetFullyQualifiedName();
        return Diagnostic.Create(Descriptor, location, typeName);
    }

    public static bool Conforming(TypeDeclarationSyntax type)
        => type.Modifiers.Any(m => m.Kind() is SyntaxKind.PartialKeyword);
}