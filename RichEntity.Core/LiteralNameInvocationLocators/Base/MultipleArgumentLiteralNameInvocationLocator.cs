using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Core.Extensions;

namespace RichEntity.Core.LiteralNameInvocationLocators.Base
{
    public abstract class MultipleArgumentLiteralNameInvocationLocator : ILiteralNameInvocationLocator
    {
        public abstract string MemberType { get; }
        protected abstract string ParameterName { get; }

        public bool IsInvocationOperationRelevant(IInvocationOperation invocationOperation, Compilation compilation)
        {
            if (!IsNameComplies(invocationOperation.TargetMethod.Name))
                return false;

            var stringType = compilation.GetTypeByMetadataName("System.String");

            if (stringType is null)
                return false;

            return invocationOperation.Arguments.Any(a => stringType.Equals(a.Value.Type));
        }

        public IArgumentOperation GetRelevantArgument(
            ImmutableArray<IArgumentOperation> arguments,
            ImmutableArray<IParameterSymbol> parameters,
            Compilation compilation)
        {
            var stringType = compilation.GetTypeByMetadataName("System.String")!;

            return arguments
                .Where(a => stringType.Equals(a.Value.Type))
                .Select((a, i) => (Argument: a, i))
                .Single(t => parameters[t.i].Name.Equals(ParameterName)).Argument;
        }

        public bool ContainsMember(ImmutableArray<ISymbol> memberSymbols, string memberName, Compilation compilation)
            => memberSymbols.ContainsPropertyFieldCalled(memberName);

        protected abstract bool IsNameComplies(string methodName);
    }
}