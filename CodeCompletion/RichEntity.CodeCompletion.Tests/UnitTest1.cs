using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using RichEntity.CodeCompletion.CodeCompletions;

namespace RichEntity.CodeCompletion.Tests;

public class Tests
{
    private string _code;
    private Document _document;

    [SetUp]
    public void Setup()
    {
        _code = File.ReadAllText("Context.cs");
        var _ = typeof(MemberNameCompletion);

        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        var workspace = new AdhocWorkspace(host);

        var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(), VersionStamp.Create(), "MyProject", "MyProject", LanguageNames.CSharp)
            .WithMetadataReferences(new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
        var project = workspace.AddProject(projectInfo);
        _document = workspace.AddDocument(project.Id, "MyFile.cs", SourceText.From(_code));
    }

    [Test]
    public async Task Test1()
    {
        var service = CompletionService.GetService(_document);
        int pos = _code.LastIndexOf("Navigation(\"", StringComparison.Ordinal) + "Navigation(\"".Length;
        var results = await service.GetCompletionsAsync(_document, pos);
        Assert.Pass();
    }
}