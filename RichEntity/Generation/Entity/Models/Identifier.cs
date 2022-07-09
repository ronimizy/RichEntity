using Microsoft.CodeAnalysis;

namespace RichEntity.Generation.Entity.Models;

public record struct Identifier(string Name, ITypeSymbol Symbol);