using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Core.EntityTypeSymbolProviders.Base;
using RichEntity.Core.Extensions;
using RichEntity.Core.LiteralNameInvocationLocators.Base;
using RichEntity.Core.Utility;

namespace RichEntity.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InvalidStringLiteralMemberNameDeclarationAnalyzer : DiagnosticAnalyzer
    {
        private readonly IReadOnlyCollection<ILiteralNameInvocationLocator> _locators;
        private readonly IReadOnlyCollection<IEntityTypeSymbolProvider> _providers;

        public InvalidStringLiteralMemberNameDeclarationAnalyzer()
        {
            _locators = AssemblyScanner.GetInstances<ILiteralNameInvocationLocator>();
            _providers = AssemblyScanner.GetInstances<IEntityTypeSymbolProvider>();
        }

        public static string Id => "RE1000";
        public static string Title => "InvalidStringLiteralMemberNameDeclaration";

        public static string Description =>
            "Type \"{0}\" does not contain the definition for {1} called \"{2}\" {3} : {4}.";

        public static DiagnosticDescriptor Descriptor { get; } = new DiagnosticDescriptor(
            Id, Title, Description, "EntityFrameworkCore", DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics |
                                                   GeneratedCodeAnalysisFlags.Analyze);

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

            var compilation = context.Compilation;

            var relevantLocator = _locators
                .SingleOrDefault(l => l.IsInvocationOperationRelevant(invocationOperation, compilation));

            if (relevantLocator is null)
                return;

            var argument = relevantLocator.GetRelevantArgument(
                invocationOperation.Arguments, invocationOperation.TargetMethod.Parameters, compilation);

            var memberName = argument.Value.ConstantValue.Value?.ToString();

            if (memberName is null)
                return;

            var genericType = invocationOperation.GetOperationUnderlyingEntityType(compilation, _providers);

            if (!(genericType is INamedTypeSymbol namedTypeSymbol))
                return;

            ImmutableArray<ISymbol> members = namedTypeSymbol.GetAllMembers();

            if (relevantLocator.ContainsMember(members, memberName, compilation))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                argument.Syntax.GetLocation(),
                namedTypeSymbol.GetFullyQualifiedName(),
                relevantLocator.MemberType,
                memberName,
                genericType.Name,
                string.Join(", ", members.Select(m => $"{m.Kind} {m.Name}"))));
        }
    }
}