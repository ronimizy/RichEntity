using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
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

            return invocationOperation.TargetMethod.Parameters
                .Any(p => stringType.EqualsProperly(p.Type) && ParameterName.Equals(p.Name));
        }

        public IArgumentOperation GetRelevantArgument(
            ImmutableArray<IArgumentOperation> arguments,
            ImmutableArray<IParameterSymbol> parameters,
            Compilation compilation)
        {
            var stringType = compilation.GetTypeByMetadataName("System.String")!;

            var index = Enumerable.Range(0, arguments.Length)
                .Where(i => stringType.EqualsProperly(parameters[i].Type))
                .Single(i => ParameterName.Equals(parameters[i].Name));

            return arguments[index];
        }

        public bool ContainsMember(ImmutableArray<ISymbol> memberSymbols, string memberName, Compilation compilation)
            => memberSymbols.ContainsPropertyFieldCalled(memberName);

        protected abstract bool IsNameComplies(string methodName);
    }
}