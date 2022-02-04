using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RichEntity.CodeCompletion.CodeCompletions;
using RichEntity.Core.Utility;

namespace RichEntity.CodeCompletion.Tests;

public class Tests
{
    private string _code;
    private Document _document;

    [SetUp]
    public void Setup()
    {
        _code = File.ReadAllText("Context.cs");

        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies.Append(typeof(MemberNameCompletion).Assembly));
        var workspace = new AdhocWorkspace(host);

        var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(), VersionStamp.Create(), "MyProject", "MyProject", LanguageNames.CSharp)
            .WithMetadataReferences(CompilationBuilder.GetAllReferencesNeededForTypes(typeof(object), typeof(ModelBuilder), typeof(MemberNameCompletion)));
        var project = workspace.AddProject(projectInfo);
        _document = project.AddDocument("MyFile.cs", SourceText.From(_code));
    }

    [Test]
    public async Task Test1()
    {
        var service = CompletionService.GetService(_document);
        int pos = _code.LastIndexOf("Navigation(\"", StringComparison.Ordinal) + "Navigation(\"".Length;
        var results = await service.GetCompletionsAsync(_document, pos);
        Assert.AreEqual(2, results.Items.Length);
    }
}