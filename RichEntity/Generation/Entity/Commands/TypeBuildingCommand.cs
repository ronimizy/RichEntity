using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RichEntity.Generation.Entity.Commands;

public record struct TypeBuildingCommand(
    TypeDeclarationSyntax Syntax,
    INamedTypeSymbol Symbol,
    ITypeSymbol IdentifierSymbol,
    TypeDeclarationSyntax Root,
    Compilation Compilation);