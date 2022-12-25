using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RichEntity.Generation.Entity.Commands;

public record struct CommentHeaderBuildingCommand(UsingDirectiveSyntax Syntax);