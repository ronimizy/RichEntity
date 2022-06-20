namespace RichEntity.Utility;

public static class Constants
{
    public const string AnnotationsNamespace = "RichEntity.Annotations";
    
    public const string EntityInterfaceName = "IEntity";
    public const string EntityInterfaceFullyQualifiedName = $"{AnnotationsNamespace}.{EntityInterfaceName}`1";
    
    public const string ConfigureConstructorsAttributeName = "ConfigureConstructorsAttribute";
    public const string ConfigureConstructorsAttributeFullyQualifiedName = $"{AnnotationsNamespace}.{ConfigureConstructorsAttributeName}";
    
    public const string ConfigureIdAttributeName = "ConfigureIdAttribute";
    public const string ConfigureIdAttributeFullyQualifiedName = $"{AnnotationsNamespace}.{ConfigureIdAttributeName}";

    public const string FilenameSuffix = "RichEntity.Generation.Entity.cs";
}