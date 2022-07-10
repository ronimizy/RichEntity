using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.FileBuildingLinks;
using RichEntity.Generation.Entity.IdentifierProviderLinks;
using RichEntity.Generation.Entity.Models;
using RichEntity.Generation.Entity.SyntaxReceivers;
using RichEntity.Generation.Entity.TypeBuildingLinks;
using RichEntity.Generation.Entity.TypePreconditionLinks;
using RichEntity.Utility;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.SourceGenerators;

[Generator]
public class EntitySourceGenerator : GeneratorBase<FileBuildingCommand>
{
    private readonly IChain<GetIdentifiersCommand, IEnumerable<Identifier>> _getIdentifiersChain;
    protected sealed override IChain<FileBuildingCommand, CompilationUnitSyntax> Chain { get; init; }

    public EntitySourceGenerator()
    {
        var collection = new ServiceCollection();

        collection
            .AddFluentChaining(o => o.ChainLifetime = ServiceLifetime.Singleton)
            .AddChain<FileBuildingCommand, CompilationUnitSyntax>
            (
                start => start
                    .Then<TypePreconditionFilter>()
                    .Then<UsingBuilder>()
                    .Then<ClassBuilder>()
                    .FinishWith((r, _) => r.Root)
            )
            .AddChain<TypeCheckingCommand, bool>
            (
                start => start
                    .Then<PartialTypeChecker>()
                    .FinishWith(() => true)
            )
            .AddChain<TypeBuildingCommand, TypeDeclarationSyntax>
            (
                start => start
                    .Then<EquatableInterfaceImplementation>()
                    .Then<IdentifierPropertyBuilder>()
                    .Then<ParametrizedConstructorBuilder>()
                    .Then<ParameterlessConstructorBuilder>()
                    .Then<EqualsBuilder>()
                    .Then<ObjectEqualsBuilder>()
                    .Then<GetHashCodeBuilder>()
                    .FinishWith((r, _) => r.Root)
            )
            .AddChain<GetIdentifiersCommand, IEnumerable<Identifier>>
            (
                start => start
                    .Then<IdentifierCachingLink>()
                    .Then<EntityIdentifierProviderLink>()
                    .Then<KeyPropertyIdentifierProvider>()
                    .FinishWith(Enumerable.Empty<Identifier>)
            );

        var provider = collection.BuildServiceProvider();

        Chain = provider.GetRequiredService<IChain<FileBuildingCommand, CompilationUnitSyntax>>();
        _getIdentifiersChain = provider.GetRequiredService<IChain<GetIdentifiersCommand, IEnumerable<Identifier>>>();
    }

    public override void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new EntitySyntaxReceiver());
    }

    protected override IReadOnlyCollection<SyntaxNode>? GetNodes(GeneratorExecutionContext context)
        => context.SyntaxReceiver is EntitySyntaxReceiver receiver ? receiver.Nodes : null;

    protected override bool TryBuildRequest(
        GeneratorExecutionContext context,
        SyntaxNode node,
        out FileBuildingCommand request)
    {
        request = default;

        if (node is not TypeDeclarationSyntax syntax)
            return false;

        var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);

        if (semanticModel.GetDeclaredSymbol(syntax) is not INamedTypeSymbol entitySymbol)
            return false;

        var getIdentifiersCommand = new GetIdentifiersCommand(entitySymbol, context.Compilation);
        IReadOnlyList<Identifier> identifiers = _getIdentifiersChain.Process(getIdentifiersCommand).ToArray();

        if (identifiers.Count is 0)
            return false;

        request = new FileBuildingCommand(
            syntax,
            entitySymbol,
            identifiers,
            CompilationUnit(),
            context);

        return true;
    }

    protected override string GetFileName(
        GeneratorExecutionContext context,
        SyntaxNode node,
        FileBuildingCommand request)
    {
        if (node is not TypeDeclarationSyntax syntax)
            return string.Empty;

        return $"{syntax.Identifier}.{Constants.FilenameSuffix}";
    }
}