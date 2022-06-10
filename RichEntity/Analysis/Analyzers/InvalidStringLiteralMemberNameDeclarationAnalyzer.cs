using System.Collections.Immutable;
using FluentScanning;
using FluentScanning.DependencyInjection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.DependencyInjection;
using RichEntity.Analysis.Extensions;
using RichEntity.Analysis.LiteralNameInvocationLocators.Base;

namespace RichEntity.Analysis.Analyzers;

// [DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InvalidStringLiteralMemberNameDeclarationAnalyzer : DiagnosticAnalyzer
{
    private readonly IReadOnlyCollection<ILiteralNameInvocationLocator> _locators;

    public InvalidStringLiteralMemberNameDeclarationAnalyzer()
    {
        var collection = new ServiceCollection();

        using (var scanner = collection.UseAssemblyScanner(typeof(IAssemblyMarker)))
        {
            scanner.EnqueueAdditionOfTypesThat()
                .WouldBeRegisteredAs<ILiteralNameInvocationLocator>()
                .WithSingletonLifetime()
                .MustBeAssignableTo<ILiteralNameInvocationLocator>()
                .AreNotInterfaces()
                .AreNotAbstractClasses();
        }

        var builder = collection.BuildServiceProvider();

        _locators = builder.GetServices<ILiteralNameInvocationLocator>().ToArray();
    }

    public static string Id => "RE1000";
    public static string Title => "InvalidStringLiteralMemberNameDeclaration";
    public static string Description => "Type \"{0}\" does not contain the definition for {1} called \"{2}\".";

    public static DiagnosticDescriptor Descriptor { get; } = new DiagnosticDescriptor(
        Id, Title, Description, "EntityFrameworkCore", DiagnosticSeverity.Error, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Descriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterCompilationStartAction(ctx =>
        {
            if (ctx.Compilation.IsEntityFrameworkCoreReferenced())
            {
                ctx.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
            }
        });
    }

    private void AnalyzeOperation(OperationAnalysisContext context)
    {
        var invocationOperation = (IInvocationOperation)context.Operation;

        if (invocationOperation.Instance is null)
            return;
        
        var relevantLocator = _locators
            .SingleOrDefault(l => l.IsInvocationOperationRelevant(invocationOperation, context));

        if (relevantLocator is null)
            return;

        var argument = relevantLocator.GetRelevantArgument(
            invocationOperation.Arguments, invocationOperation.TargetMethod.Parameters, context);

        var memberName = argument.Value.ConstantValue.Value?.ToString();

        if (memberName is null)
            return;

        var genericType = invocationOperation.GetOperationUnderlyingEntityType(context.Compilation);

        if (!(genericType is INamedTypeSymbol namedTypeSymbol))
            return;

        if (relevantLocator.ContainsMember(namedTypeSymbol.GetMembers(), memberName, context))
            return;

        context.ReportDiagnostic(Diagnostic.Create(
            Descriptor,
            argument.Syntax.GetLocation(),
            namedTypeSymbol.GetFullyQualifiedName(),
            relevantLocator.MemberType,
            memberName));
    }
}