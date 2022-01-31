using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace RichEntity.Analyzers.EntityTypeSymbolProviders
{
    public class ModelBuilderTypeSymbolProvider : IEntityTypeSymbolProvider
    {
        private const string FullyQualifiedName = "Microsoft.EntityFrameworkCore.ModelBuilder";

        public bool TryGetTypeSymbol(IOperation operation, Compilation compilation, out ITypeSymbol? symbol)
        {
            var modelBuilderType = compilation.GetTypeByMetadataName(FullyQualifiedName);
            if (operation is IInvocationOperation invocationOperation &&
                invocationOperation.Instance is IParameterReferenceOperation parameterReferenceOperation &&
                invocationOperation.TargetMethod.Name.Equals("Entity") &&
                (parameterReferenceOperation.Type?.Equals(modelBuilderType) ?? false))
            {
                symbol = invocationOperation.TargetMethod.TypeArguments.Single();
                return true;
            }

            symbol = null;
            return false;
        }
    }
}