using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Annotations;
using RichEntity.Extensions;
using Sigil;

namespace RichEntity.ObjectCreation;

public static class ObjectCreator
{
    private record struct MemberKey(Type Type, string MemberName);

    private static readonly ConcurrentDictionary<MemberKey, Func<object?, Action<object>>> AssignmentCache =
        new ConcurrentDictionary<MemberKey, Func<object?, Action<object>>>();

    private static readonly Assembly AnnotationsAssembly;

    static ObjectCreator()
    {
        AnnotationsAssembly = typeof(IEntity).Assembly;
    }

    public static object Create(SyntaxNode syntax, Compilation compilation)
    {
        var model = compilation.GetSemanticModel(syntax.SyntaxTree);
        var operation = model.GetOperation(syntax);

        if (operation is null)
            throw new InvalidOperationException();

        if (operation.Type is null)
            throw new InvalidOperationException();

        var typeName = operation.Type.GetFullyQualifiedName();

        var type = AnnotationsAssembly.GetType(typeName);

        if (type is null)
            throw new InvalidOperationException();

        return Create(type, operation);
    }

    public static T Create<T>(SyntaxNode syntax, Compilation compilation)
    {
        var model = compilation.GetSemanticModel(syntax.SyntaxTree);
        var operation = model.GetOperation(syntax);
        var type = typeof(T);

        if (operation is null)
            throw new InvalidOperationException();

        return (T)Create(type, operation);
    }

    private static object Create(Type type, IOperation operation)
    {
        var instance = operation is IObjectCreationOperation creationOperation
            ? Activator.CreateInstance(type, GetParameters(creationOperation))
            : Activator.CreateInstance(type);

        IEnumerable<Action<object>> assignmentOperations = operation
            .Descendants()
            .OfType<ISimpleAssignmentOperation>()
            .Select(x => GetAssignmentAction(type, x));

        foreach (Action<object> action in assignmentOperations)
        {
            action.Invoke(instance);
        }

        return instance;
    }

    private static object?[] GetParameters(IObjectCreationOperation operation)
    {
        if (operation.Arguments.Any(x => x.Value is not ILiteralOperation))
            throw new InvalidOperationException();

        return operation.Arguments.Select(x => x.Value.ConstantValue.Value).ToArray();
    }

    private static Action<object> GetAssignmentAction(Type type, IAssignmentOperation operation)
    {
        if (operation.Target is not IMemberReferenceOperation memberReferenceOperation)
            throw new NotSupportedException();

        var memberName = memberReferenceOperation.Member.Name;
        var key = new MemberKey(type, memberName);

        Func<object?, Action<object>> action = AssignmentCache.GetOrAdd(key, GetAssignmentFunction);

        return action.Invoke(operation.Value.ConstantValue.Value);
    }

    private static Func<object?, Action<object>> GetAssignmentFunction(MemberKey key)
    {
        var member = key.Type.GetMember(key.MemberName).SingleOrDefault();

        Action<object, object?> action = member switch
        {
            PropertyInfo property => CreateMethodForProperty(key, property),
            FieldInfo field => CreateMethodForField(key, field),
            _ => throw new NotSupportedException(),
        };

        return arg => instance => action.Invoke(instance, arg);
    }

    private static Action<object, object?> CreateMethodForProperty(MemberKey key, PropertyInfo info)
    {
        File.WriteAllText("/Users/george/Desktop/file.txt", "adawdwad");

        Emit<Action<object, object?>> emit = Emit<Action<object, object?>>
            .NewDynamicMethod(info.SetMethod.Name)
            .LoadArgument(0);

        emit = key.Type.IsValueType
            ? emit.Unbox(key.Type)
            : emit.CastClass(key.Type);

        emit = emit.LoadArgument(1);

        emit = info.PropertyType.IsValueType
            ? emit.UnboxAny(info.PropertyType)
            : emit.CastClass(info.PropertyType);

        return emit.Call(info.SetMethod).Return().CreateDelegate();
    }

    private static Action<object, object?> CreateMethodForField(MemberKey key, FieldInfo info)
    {
        Emit<Action<object, object?>> emit = Emit<Action<object, object?>>
            .NewDynamicMethod()
            .LoadArgument(0);

        emit = key.Type.IsValueType
            ? emit.Unbox(key.Type)
            : emit.CastClass(key.Type);

        emit = emit.LoadArgument(1);

        emit = info.FieldType.IsValueType
            ? emit.UnboxAny(info.FieldType)
            : emit.CastClass(info.FieldType);

        return emit.StoreField(info).Return().CreateDelegate();
    }
}