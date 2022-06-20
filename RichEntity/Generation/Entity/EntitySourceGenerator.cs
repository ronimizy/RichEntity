using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.FileBuildingLinks;
using RichEntity.Generation.Entity.TypeBuildingLinks;
using RichEntity.Generation.Entity.TypePreconditionLinks;
using RichEntity.Utility;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity;

[Generator]
public class EntitySourceGenerator : ISourceGenerator
{
    private readonly IChain<FileBuildingCommand, CompilationUnitSyntax> _fileBuildingChain;

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

        _fileBuildingChain = provider.GetRequiredService<IChain<FileBuildingCommand, CompilationUnitSyntax>>();
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new EntityInterfaceSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (context.SyntaxReceiver is not EntityInterfaceSyntaxReceiver receiver)
            return;

        var entityInterface = context.Compilation
            .GetTypeByMetadataName(Constants.EntityInterfaceFullyQualifiedName);

        if (entityInterface is null)
            return;

        foreach (var syntax in receiver.Nodes)
        {
            ProcessEntity(context, entityInterface, syntax);
        }
    }

    private void ProcessEntity(
        GeneratorExecutionContext context,
        INamedTypeSymbol entityInterface,
        TypeDeclarationSyntax syntax)
    {
        var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);

        if (semanticModel.GetDeclaredSymbol(syntax) is not INamedTypeSymbol entitySymbol)
            return;

        var concreteEntityInterface = entitySymbol.Interfaces
            .SingleOrDefault(t => t.IsAssignableTo(entityInterface));

        var identifierSymbol = concreteEntityInterface?.TypeArguments.SingleOrDefault();

        if (identifierSymbol is null)
            return;

        var fileBuildingCommand = new FileBuildingCommand(
            syntax,
            entitySymbol,
            identifierSymbol,
            CompilationUnit(),
            context);

        var compilationUnit = _fileBuildingChain.Process(fileBuildingCommand).NormalizeWhitespace();
        var fileName = $"{syntax.Identifier}.{Constants.FilenameSuffix}";

        context.AddSource(fileName, compilationUnit.ToString());
    }
}