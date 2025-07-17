namespace Dsr.Architecture.Infrastructure.Persistence.Mongo;

/// <summary>
/// Attribute to specify the MongoDB collection name for a class.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BsonCollectionAttribute"/> class.
/// </remarks>
/// <param name="collectionName">The name of the MongoDB collection.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BsonCollectionAttribute(string collectionName) : Attribute
{
    /// <summary>
    /// Gets the name of the MongoDB collection.
    /// </summary>
    public string CollectionName { get; } = collectionName;
}

/// <summary>
/// Extension methods for the <see cref="BsonCollectionAttribute"/>.
/// </summary>
public static class BsonCollectionAttributeExtend
{
    /// <summary>
    /// Gets the collection name specified by the <see cref="BsonCollectionAttribute"/> on a type.
    /// </summary>
    /// <param name="documentType">The type to get the collection name for.</param>
    /// <returns>The collection name if the attribute is present; otherwise, null.</returns>
    public static string? GetCollectionName(this Type documentType)
        => (documentType.GetCustomAttributes(typeof(BsonCollectionAttribute), true)
            .FirstOrDefault() as BsonCollectionAttribute)?.CollectionName;
}
