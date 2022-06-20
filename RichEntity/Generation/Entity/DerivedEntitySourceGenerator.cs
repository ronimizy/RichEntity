using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.FileBuildingLinks;
using RichEntity.Generation.Entity.TypeBuildingLinks;
using RichEntity.Generation.Entity.TypePreconditionLinks;

namespace RichEntity.Generation.Entity;

[Generator]
public class DerivedEntitySourceGenerator : EntitySourceGenerator
{
    public DerivedEntitySourceGenerator()
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
                    .Then<ParameterlessConstructorBuilder>()
                    .Then<EqualsBuilder>()
                    .Then<ObjectEqualsBuilder>()
                    .Then<GetHashCodeBuilder>()
                    .FinishWith((r, c) => r.Root)
            );

        var provider = collection.BuildServiceProvider();

        FileBuildingChain = provider.GetRequiredService<IChain<FileBuildingCommand, CompilationUnitSyntax>>();
    }

    public override void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new DerivedEntityInterfaceSyntaxReceiver());
    }

    protected override IReadOnlyCollection<TypeDeclarationSyntax>? GetNodes(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not DerivedEntityInterfaceSyntaxReceiver receiver)
            return null;
        
        return receiver.Nodes;
    }

    protected override ITypeSymbol? GetIdentifierSymbol(INamedTypeSymbol entitySymbol, INamedTypeSymbol entityInterface)
    {
        var concreteEntityInterface = entitySymbol.AllInterfaces
            .SingleOrDefault(t => t.IsAssignableTo(entityInterface));

        return concreteEntityInterface?.TypeArguments.SingleOrDefault();
    }
}