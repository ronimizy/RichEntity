using FluentChaining;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class EquatableInterfaceImplementation : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private static readonly GenericNameSyntax GenericName = GenericName(Identifier("IEquatable"));

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        var equatableInterface = request.Compilation
            .GetTypeByMetadataName("System.IEquatable`1")?
            .Construct(request.Symbol);

        if (equatableInterface is not null && request.Symbol.Interfaces.Contains(equatableInterface))
            return next(request, context);

        var argument = SingletonSeparatedList<TypeSyntax>(IdentifierName(request.Symbol.Name));
        var interfaceName = GenericName.WithTypeArgumentList(TypeArgumentList(argument));
        var baseTypes = (request.Root.BaseList ?? BaseList()).Types.Append(SimpleBaseType(interfaceName));

        request = request with
        {
            Root = request.Root.WithBaseList(BaseList(SeparatedList(baseTypes)))
        };

        return next(request, context);
    }
}