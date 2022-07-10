using RichEntity.Annotations;

namespace RichEntity.Utility;

public static class Constants
{
    public const string AnnotationsNamespace = "RichEntity.Annotations";
    
    public const string EntityInterfaceName = nameof(IEntity);
    public const string EntityInterfaceFullyQualifiedName = $"{AnnotationsNamespace}.{EntityInterfaceName}`1";
    
    public const string CompositeEntityInterfaceFullyQualifiedName = $"{AnnotationsNamespace}.{EntityInterfaceName}";
    
    public const string ConfigureConstructorsAttributeName = nameof(ConfigureConstructorsAttribute);
    public const string ConfigureConstructorsAttributeFullyQualifiedName = $"{AnnotationsNamespace}.{ConfigureConstructorsAttributeName}";
    
    public const string ConfigureIdAttributeName = nameof(ConfigureIdAttribute);
    public const string ConfigureIdAttributeFullyQualifiedName = $"{AnnotationsNamespace}.{ConfigureIdAttributeName}";

    public const string KeyPropertyAttributeName = nameof(KeyPropertyAttribute);
    public const string KeyEntityAttributeFullyQualifiedName = $"{AnnotationsNamespace}.{KeyPropertyAttributeName}";

    public const string FilenameSuffix = "RichEntity.Generation.Entity.cs";
}