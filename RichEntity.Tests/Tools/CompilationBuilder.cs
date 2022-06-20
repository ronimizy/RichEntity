using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace RichEntity.Analyzers.Tests.Tools;

public static class CompilationBuilder
{
    public static Task<Compilation> BuildCompilation(
        IReadOnlyCollection<Type> referencedTypes,
        params SourceFile[] sourceFiles)
        => BuildCompilation(referencedTypes, null, null, sourceFiles);

    public static async Task<Compilation> BuildCompilation(
        IReadOnlyCollection<Type> referencedTypes,
        CompilationOptions? compilationOptions = null,
        ParseOptions? parseOptions = null,
        params SourceFile[] sourceFiles)
    {
        var project = BuildProject(referencedTypes, compilationOptions, parseOptions, sourceFiles);
        return (await project.GetCompilationAsync())!;
    }

    public static Project BuildProject(
        IReadOnlyCollection<Type> referencedTypes,
        CompilationOptions? compilationOptions = null,
        ParseOptions? parseOptions = null,
        params SourceFile[] sourceFiles)
    {
        var workspace = new AdhocWorkspace();
        var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

        var solution = workspace.CurrentSolution;
        var projectId = ProjectId.CreateNewId();

        solution = solution.AddProject(projectId, "MyTestProject", "MyTestProject", LanguageNames.CSharp);

        foreach (var (name, code) in sourceFiles)
        {
            solution = solution.AddDocument(DocumentId.CreateNewId(projectId), name, code);
        }

        if (parseOptions is not null)
        {
            solution = solution.WithProjectParseOptions(projectId, parseOptions);
        }

        var project = solution.GetProject(projectId)!;

        if (compilationOptions is not null)
        {
            project = project.WithCompilationOptions(compilationOptions);
        }
        
        project = project.AddMetadataReferences(GetAllReferencesNeededForTypes(referencedTypes));

        workspace.TryApplyChanges(project.Solution);

        return workspace.CurrentSolution.Projects.Single();
    }

    private static IEnumerable<MetadataReference> GetAllReferencesNeededForTypes(IReadOnlyCollection<Type> types)
    {
        IEnumerable<string> files = types.SelectMany(GetAllAssemblyFilesNeededForType);
        return files.Select(x => (MetadataReference)MetadataReference.CreateFromFile(x));
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