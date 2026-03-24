using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Enums;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Analytics;

/// <summary>
/// Factory for creating dynamic types at runtime to hold GroupBy aggregation results.
/// These types are necessary because EF Core requires concrete types for query translation,
/// and anonymous types cannot be used across method boundaries in a strongly-typed manner.
/// </summary>
public static class DynamicTypeFactory
{
    private static readonly ConcurrentDictionary<string, Type> _typeCache = new();

    /// <summary>
    /// Gets or creates a dynamic type for GroupBy result with the specified key type and aggregations.
    /// </summary>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="aggregations">The list of aggregations to include in the result type.</param>
    /// <returns>A dynamically created type with Key and aggregation properties.</returns>
    public static Type GetOrCreateGroupByResultType<TKey>(IReadOnlyList<AggregationDefinition> aggregations)
    {
        var typeName = GenerateTypeName<TKey>(aggregations);

        return _typeCache.GetOrAdd(typeName, _ => CreateGroupByResultType<TKey>(typeName, aggregations));
    }

    /// <summary>
    /// Generates a unique type name based on the key type and aggregations.
    /// </summary>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="aggregations">The list of aggregations.</param>
    /// <returns>A unique string representing the type name.</returns>
    private static string GenerateTypeName<TKey>(IReadOnlyList<AggregationDefinition> aggregations)
    {
        var keyTypeName = typeof(TKey).Name;
        var aggregationSignature = string.Join("_", aggregations.Select(a => $"{a.Alias}_{a.Type}"));
        return $"GroupByResult_{keyTypeName}_{aggregationSignature}";
    }

    /// <summary>
    /// Creates a dynamic type using reflection emit.
    /// </summary>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="typeName">The name of the type to create.</param>
    /// <param name="aggregations">The list of aggregations to include as properties.</param>
    /// <returns>The newly created dynamic Type.</returns>
    private static Type CreateGroupByResultType<TKey>(string typeName, IReadOnlyList<AggregationDefinition> aggregations)
    {
        var assemblyName = new AssemblyName($"DynamicTypes_{Guid.NewGuid():N}");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicTypesModule");

        var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);

        // Add Key property
        var keyType = typeof(TKey);
        AddProperty(typeBuilder, "Key", keyType);

        // Add aggregation properties
        foreach (var aggregation in aggregations)
        {
            var propertyType = GetAggregationPropertyType(aggregation);
            AddProperty(typeBuilder, aggregation.Alias, propertyType);
        }

        return typeBuilder.CreateType()!;
    }

    /// <summary>
    /// Adds a property with a backing field and getter/setter to the type builder.
    /// </summary>
    /// <param name="typeBuilder">The type builder to add the property to.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyType">The type of the property.</param>
    private static void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
    {
        var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

        var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, []);

        // Getter
        var getterBuilder = typeBuilder.DefineMethod($"get_{propertyName}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            propertyType, Type.EmptyTypes);

        var getterIl = getterBuilder.GetILGenerator();
        getterIl.Emit(OpCodes.Ldarg_0);
        getterIl.Emit(OpCodes.Ldfld, fieldBuilder);
        getterIl.Emit(OpCodes.Ret);
        propertyBuilder.SetGetMethod(getterBuilder);

        // Setter
        var setterBuilder = typeBuilder.DefineMethod($"set_{propertyName}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            null, [propertyType]);

        var setterIl = setterBuilder.GetILGenerator();
        setterIl.Emit(OpCodes.Ldarg_0);
        setterIl.Emit(OpCodes.Ldarg_1);
        setterIl.Emit(OpCodes.Stfld, fieldBuilder);
        setterIl.Emit(OpCodes.Ret);
        propertyBuilder.SetSetMethod(setterBuilder);
    }

    /// <summary>
    /// Gets the CLR type for an aggregation result property.
    /// </summary>
    /// <param name="aggregation">The aggregation definition.</param>
    /// <returns>The result Type for the specified aggregation.</returns>
    private static Type GetAggregationPropertyType(AggregationDefinition aggregation)
    {
        return aggregation.Type switch
        {
            AggregationType.Count => typeof(int),
            AggregationType.Sum => GetNonNullableType(aggregation.Selector.ReturnType),
            AggregationType.Avg => typeof(double),
            AggregationType.Max => aggregation.Selector.ReturnType,
            AggregationType.Min => aggregation.Selector.ReturnType,
            _ => typeof(object)
        };
    }

    /// <summary>
    /// Gets the non-nullable underlying type if the type is nullable.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>The non-nullable underlying type, or the original type if it wasn't nullable.</returns>
    private static Type GetNonNullableType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Nullable.GetUnderlyingType(type)!;
        }
        return type;
    }

    /// <summary>
    /// Clears the type cache. Useful for testing.
    /// </summary>
    public static void ClearCache()
    {
        _typeCache.Clear();
    }
}
