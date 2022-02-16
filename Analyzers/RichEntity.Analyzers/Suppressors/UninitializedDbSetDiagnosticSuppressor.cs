using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RichEntity.Core.Extensions;

namespace RichEntity.Analyzers.Suppressors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UninitializedDbSetDiagnosticSuppressor : DiagnosticSuppressor
    {
        private readonly SuppressionDescriptor _descriptor = new SuppressionDescriptor(
            "RE2000", "CS8618", "DbSet<> is automatically initialized in DbContext.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions =>
            ImmutableArray.Create(_descriptor);


        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            if (!context.Compilation.IsEntityFrameworkCoreReferenced())
                return;

            INamedTypeSymbol? contextType = null;
            INamedTypeSymbol? setType = null;

            var diagnostics = context.ReportedDiagnostics
                .Where(d => !d.IsSuppressed)
                .Where(d => d.Location.SourceTree is object);

            foreach (var diagnostic in diagnostics)
            {
                var tree = diagnostic.Location.SourceTree!;
                var model = context.GetSemanticModel(tree);
                var node = tree.GetRoot().FindNode(diagnostic.Location.SourceSpan);
                var symbol = model.GetDeclaredSymbol(node);
                
                if (!(symbol is IMethodSymbol methodSymbol))
                    continue;

                var receiverType = methodSymbol.ReceiverType;
                
                if (!(receiverType is INamedTypeSymbol namedReceiverType))
                    continue;

                contextType ??= context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbContext");
                
                if (contextType is null)
                    return;
                
                if (!namedReceiverType.DerivesFrom(contextType))
                    continue;

                var argumentName = ParseArgumentName(diagnostic.ToString());
                var argument = receiverType.GetMembers(argumentName).FirstOrDefault();
                
                if (!(argument is IPropertySymbol { Type: INamedTypeSymbol namedArgumentType }))
                    continue;
                
                setType ??= context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbSet`1");
                
                if (setType is null)
                    return;
                
                if (!namedArgumentType.ConstructedFrom.DerivesOrConstructedFrom(setType))
                    continue;
                
                context.ReportSuppression(Suppression.Create(_descriptor, diagnostic));
            }
        }

        private static string ParseArgumentName(string formattedTitle)
        {
            var start = formattedTitle.IndexOf('\'') + 1;
            var end = formattedTitle.IndexOf('\'', start);

            return formattedTitle[start..end];
        }
    }
}