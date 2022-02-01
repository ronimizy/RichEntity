using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RichEntity.Analyzers.Utility
{
    public static class AssemblyScanner
    {
        public static IReadOnlyCollection<T> GetInstances<T>(IServiceCollection? dependencies = null)
        {
            var collection = new ServiceCollection();

            if (dependencies is { })
            {
                foreach (var descriptor in dependencies)
                {
                    collection.Add(descriptor);
                }
            }

            var type = typeof(T);

            IReadOnlyCollection<Type> types = Locate(type, type.Assembly);

            foreach (var injectedType in types)
            {
                collection.AddTransient(injectedType);
            }

            var provider = collection.BuildServiceProvider();
            return types.Select(t => (T)provider.GetRequiredService(t)).ToArray();
        }

        private static IReadOnlyCollection<Type> Locate(Type typeToLocate, params Assembly[] assemblies)
        {
            return assemblies
                .SelectMany(a => a.DefinedTypes)
                .Where(typeToLocate.IsAssignableFrom)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .ToArray();
        }
    }
}