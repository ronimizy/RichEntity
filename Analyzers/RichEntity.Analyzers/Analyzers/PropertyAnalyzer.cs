using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Analyzers.Extensions;

namespace RichEntity.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PropertyAnalyzer : DiagnosticAnalyzer
    {
        public static string Id => "RE1001";
        public static string Title => "Invalid \".Property()\" declaration.";
        public static string Description => "Type \"{0}\" does not contain the definition for property called \"{1}\".";

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

            if (!invocationOperation.TargetMethod.Name.Equals("Property"))
                return;

            var stringType = context.Compilation.GetTypeByMetadataName("System.String");

            if (stringType is null)
                return;

            var argument = invocationOperation.Arguments
                .SingleOrDefault(a => stringType.Equals(a.Value.Type));

            var propertyName = argument?.Value.ConstantValue.Value?.ToString();

            if (propertyName is null)
                return;

            var genericType = invocationOperation.Instance.GetOperationUnderlyingEntityType(context.Compilation);

            if (!(genericType is INamedTypeSymbol namedTypeSymbol))
                return;

            if (namedTypeSymbol.MemberNames.Contains(propertyName))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                argument?.Syntax.GetLocation() ?? invocationOperation.Syntax.GetLocation(),
                namedTypeSymbol.GetFullyQualifiedName(),
                propertyName));
        }
    }
}