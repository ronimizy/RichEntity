using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Core.EntityTypeSymbolProviders.Base;
using RichEntity.Core.Extensions;
using RichEntity.Core.LiteralNameInvocationLocators.Base;
using RichEntity.Core.Utility;

namespace RichEntity.CodeCompletion.CodeCompletions
{
    [ExportCompletionProvider(nameof(MemberNameCompletion), LanguageNames.CSharp)]
    [Shared]
    public class MemberNameCompletion : CompletionProvider
    {
        private readonly IReadOnlyCollection<ILiteralNameInvocationLocator> _locators;
        private readonly IReadOnlyCollection<IEntityTypeSymbolProvider> _providers;

        [ImportingConstructor]
        public MemberNameCompletion()
        {
            _locators = AssemblyScanner.GetInstances<ILiteralNameInvocationLocator>();
            _providers = AssemblyScanner.GetInstances<IEntityTypeSymbolProvider>();
        }

        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            var model = await context.Document.GetSemanticModelAsync();
            if (model is null)
                return;

            var tree = model.SyntaxTree;
            var root = await tree.GetRootAsync();

            var nodes = root
                .DescendantNodes(n => n.Span.Start <= context.Position && n.Span.End >= context.Position)
                .OfType<InvocationExpressionSyntax>();

            var operations = nodes
                .Select(n => model.GetOperation(n))
                .OfType<IInvocationOperation>()
                .Where(o => o.Instance is object);

            foreach (var operation in operations)
            {
                var relevantLocator = _locators.FirstOrDefault(l => 
                    l.IsInvocationOperationRelevant(operation, model.Compilation));
                
                if (relevantLocator is null)
                    continue;
                
                var argument = relevantLocator.GetRelevantArgument(
                    operation.Arguments, operation.TargetMethod.Parameters, model.Compilation);

                var argumentName = argument.Value.ConstantValue.Value?.ToString();
                
                if (argumentName is null)
                    continue;

                var genericType = operation.GetOperationUnderlyingEntityType(model.Compilation, _providers);
                
                if (!(genericType is INamedTypeSymbol namedTypeSymbol))
                    continue;

                var members = namedTypeSymbol
                    .GetMembers()
                    .WhereSymbolsArePropertyFields()
                    .Select(m => (m.Name, Distance: LevenshteinDistance.Calculate(argumentName, m.Name) / m.Name.Length))
                    .Where(t => t.Distance < 1)
                    .OrderBy(t => t.Distance)
                    .Select(t => t.Name);

                var items = members.Select(n => CompletionItem.Create(n));

                context.AddItems(items);
            }
        }
    }
}