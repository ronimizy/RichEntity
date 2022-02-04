using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace RichEntity.Core.LiteralNameInvocationLocators.Base
{
    public interface ILiteralNameInvocationLocator
    {
        string MemberType { get; }

        bool IsInvocationOperationRelevant(IInvocationOperation invocationOperation, Compilation compilation);

        IArgumentOperation GetRelevantArgument(
            ImmutableArray<IArgumentOperation> arguments,
            ImmutableArray<IParameterSymbol> parameters,
            Compilation compilation);

        bool ContainsMember(ImmutableArray<ISymbol> memberSymbols, string memberName, Compilation compilation);
    }
}