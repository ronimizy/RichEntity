using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace RichEntity.Analyzers.LiteralNameInvocationLocators.Base
{
    public interface ILiteralNameInvocationLocator
    {
        string MemberType { get; }

        bool IsInvocationOperationRelevant(IInvocationOperation invocationOperation, OperationAnalysisContext context);

        IArgumentOperation GetRelevantArgument(
            ImmutableArray<IArgumentOperation> arguments, OperationAnalysisContext context);

        bool ContainsMember(ImmutableArray<ISymbol> memberSymbols, string memberName, OperationAnalysisContext context);
    }
}