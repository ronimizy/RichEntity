using System;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace RichEntity.CodeCompletion.CodeCompletions
{
    [ExportCompletionProvider(nameof(MemberNameCompletion), LanguageNames.CSharp)]
    [Shared]
    public class MemberNameCompletion : CompletionProvider
    {
        public override bool ShouldTriggerCompletion(
            SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options)
            => true;

        public override Task ProvideCompletionsAsync(CompletionContext context)
        {
            Console.WriteLine();
            return Task.CompletedTask;
        }
    }
}