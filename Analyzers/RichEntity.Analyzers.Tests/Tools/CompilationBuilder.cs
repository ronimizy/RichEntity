using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RichEntity.Analyzers.Tests.Tools;

public static class CompilationBuilder
{
    public static async Task<Compilation> Build(string code, params Type[] referencedTypes)
    {
        var workspace = new AdhocWorkspace();
        var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

        var solution = workspace.CurrentSolution;

        var projectId = ProjectId.CreateNewId();

        solution = solution
            .AddProject(
                projectId,
                "MyTestProject",
                "MyTestProject",
                LanguageNames.CSharp);

        solution = solution
            .AddDocument(DocumentId.CreateNewId(projectId),
                "File.cs",
                code);

        var project = solution.GetProject(projectId)!;
        var options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable);

        project = project.WithCompilationOptions(options);
        project = project.AddMetadataReferences(GetAllReferencesNeededForTypes(referencedTypes));

        return (await project.GetCompilationAsync())!;
    }

    private static MetadataReference[] GetAllReferencesNeededForTypes(params Type[] types)
    {
        var files = types.SelectMany(GetAllAssemblyFilesNeededForType);
        return files.Select(x => (MetadataReference)MetadataReference.CreateFromFile(x)).ToArray();
    }

    private static string[] GetAllAssemblyFilesNeededForType(Type type)
    {
        return type.Assembly.GetReferencedAssemblies()
            .Select(x => Assembly.Load(x.FullName))
            .Append(type.Assembly)
            .Select(x => x.Location)
            .ToArray();
    }
}