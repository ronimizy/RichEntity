using FluentScanning;
using FluentScanning.DependencyInjection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.DependencyInjection;
using RichEntity.EntityFrameworkCore.Analysis.EntityTypeSymbolProviders.Base;

namespace RichEntity.EntityFrameworkCore.Analysis.Extensions;

public static class OperationExtensions
{
    public static ITypeSymbol? GetOperationUnderlyingEntityType(this IOperation operation, Compilation compilation)
    {
        var collection = new ServiceCollection();

        using (var scanner = collection.UseAssemblyScanner(typeof(IAssemblyMarker)))
        {
            scanner.EnqueueAdditionOfTypesThat()
                .WouldBeRegisteredAs<IEntityTypeSymbolProvider>()
                .WithSingletonLifetime()
                .MustBeAssignableTo<IEntityTypeSymbolProvider>()
                .AreNotAbstractClasses()
                .AreNotInterfaces();
        }

        IEnumerable<IEntityTypeSymbolProvider> providers = collection
            .BuildServiceProvider()
            .GetServices<IEntityTypeSymbolProvider>()
            .ToArray();

        while (true)
        {
            foreach (var provider in providers)
            {
                if (provider.TryGetTypeSymbol(operation, compilation, out var symbol))
                {
                    return symbol;
                }
            }

            if (operation is IInvocationOperation { Instance: { } instance })
            {
                operation = instance;
                continue;
            }

            return null;
        }
    }
}