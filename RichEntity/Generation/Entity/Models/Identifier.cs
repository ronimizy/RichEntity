using Microsoft.CodeAnalysis;

namespace RichEntity.Generation.Entity.Models;

public record struct Identifier(ITypeSymbol Symbol);