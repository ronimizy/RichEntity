using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Models;

namespace RichEntity.Generation.Entity.Commands;

public record struct TypeCheckingCommand(
    TypeDeclarationSyntax Syntax,
    INamedTypeSymbol Symbol,
    IReadOnlyCollection<Identifier> Identifiers,
    GeneratorExecutionContext Context);