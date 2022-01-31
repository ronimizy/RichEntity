using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Analyzers.Extensions;

namespace RichEntity.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class HasFieldAnalyzer : DiagnosticAnalyzer
    {
        public static string Id => "RE1000";
        public static string Title => "Invalid \".HasField()\" declaration.";
        public static string Description => "Type \"{0}\" does not contain the definition for field called \"{1}\".";

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

            if (!invocationOperation.TargetMethod.Name.Equals("HasField"))
                return;

            if (!(invocationOperation.Arguments.Length is 1))
                return;

            var fieldName = invocationOperation.Arguments[0].Value.ConstantValue.Value?.ToString();

            if (fieldName is null)
                return;

            var genericType = invocationOperation.GetOperationUnderlyingEntityType(context.Compilation);

            if (!(genericType is  INamedTypeSymbol namedTypeSymbol))
                return;

            if (namedTypeSymbol.MemberNames.Contains(fieldName))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                invocationOperation.Arguments[0].Syntax.GetLocation(),
                namedTypeSymbol.GetFullyQualifiedName(),
                fieldName));
        }
    }
}