using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Analyzers.Extensions;
using RichEntity.Analyzers.LiteralNameInvocationLocators.Base;
using RichEntity.Analyzers.Utility;

namespace RichEntity.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InvalidStringLiteralMemberNameDeclarationAnalyzer : DiagnosticAnalyzer
    {
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

            var invocationLocators = AssemblyScanner.GetInstances<ILiteralNameInvocationLocator>();

            var relevantLocator = invocationLocators
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
}