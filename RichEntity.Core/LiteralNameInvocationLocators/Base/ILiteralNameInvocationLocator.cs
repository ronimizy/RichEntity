using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace RichEntity.Core.LiteralNameInvocationLocators.Base
{
    public interface ILiteralNameInvocationLocator
    {
        string MemberType { get; }

        bool IsInvocationOperationRelevant(IInvocationOperation invocationOperation, OperationAnalysisContext context);

        IArgumentOperation GetRelevantArgument(
            ImmutableArray<IArgumentOperation> arguments,
            ImmutableArray<IParameterSymbol> parameters,
            OperationAnalysisContext context);

        bool ContainsMember(ImmutableArray<ISymbol> memberSymbols, string memberName, OperationAnalysisContext context);
    }
}