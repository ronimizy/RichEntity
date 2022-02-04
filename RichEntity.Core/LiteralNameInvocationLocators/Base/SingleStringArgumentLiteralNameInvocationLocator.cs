using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Core.Extensions;

namespace RichEntity.Core.LiteralNameInvocationLocators.Base
{
    public abstract class SingleStringArgumentLiteralNameInvocationLocator : ILiteralNameInvocationLocator
    {
        public abstract string MemberType { get; }

        public bool IsInvocationOperationRelevant(IInvocationOperation invocationOperation, Compilation compilation)
        {
            if (!IsNameComplies(invocationOperation.TargetMethod.Name))
                return false;

            if (!(invocationOperation.Arguments.Length is 1))
                return false;

            return true;
        }

        public IArgumentOperation GetRelevantArgument(
            ImmutableArray<IArgumentOperation> arguments, ImmutableArray<IParameterSymbol> parameters,
            Compilation compilation)
            => arguments[0];

        public bool ContainsMember(ImmutableArray<ISymbol> memberSymbols, string memberName, Compilation compilation)
            => memberSymbols.ContainsPropertyFieldCalled(memberName);

        protected abstract bool IsNameComplies(string methodName);
    }
}