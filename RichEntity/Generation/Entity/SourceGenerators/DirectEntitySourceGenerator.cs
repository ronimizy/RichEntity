using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.FileBuildingLinks;
using RichEntity.Generation.Entity.Models;
using RichEntity.Generation.Entity.SyntaxReceivers;
using RichEntity.Generation.Entity.TypeBuildingLinks;
using RichEntity.Generation.Entity.TypePreconditionLinks;
using RichEntity.Utility;

namespace RichEntity.Generation.Entity.SourceGenerators;

public class DirectEntitySourceGenerator : EntitySourceGeneratorBase
{
    public DirectEntitySourceGenerator()
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
                    .FinishWith((r, c) => r.Root)
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
                    .FinishWith((r, c) => r.Root)
            );

        var provider = collection.BuildServiceProvider();

        Chain = provider.GetRequiredService<IChain<FileBuildingCommand, CompilationUnitSyntax>>();
    }

    public override void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new EntityInterfaceSyntaxReceiver());
    }

    protected override IReadOnlyCollection<SyntaxNode>? GetNodes(GeneratorExecutionContext context)
        => context.SyntaxReceiver is EntityInterfaceSyntaxReceiver receiver ? receiver.Nodes : null;

    protected override IReadOnlyCollection<Identifier> GetIdentifiers(
        GeneratorExecutionContext context,
        INamedTypeSymbol entitySymbol,
        SemanticModel semanticModel)
    {
        var entityInterface = context.Compilation
            .GetTypeByMetadataName(Constants.EntityInterfaceFullyQualifiedName);

        if (entityInterface is null)
            return Array.Empty<Identifier>();

        var concreteEntityInterface = entitySymbol.Interfaces
            .SingleOrDefault(t => t.IsAssignableTo(entityInterface));
        
        if (concreteEntityInterface?.TypeArguments.Length is not 1)
            return Array.Empty<Identifier>();

        return new[]
        {
            new Identifier("Id", concreteEntityInterface.TypeArguments[0])
        };
    }
}