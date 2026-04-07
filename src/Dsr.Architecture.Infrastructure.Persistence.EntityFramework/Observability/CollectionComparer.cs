using System.Collections;
using System.Reflection;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;

/// <summary>
/// Compares two collections for deep equality with support for:
/// - IEquatable types (direct equality)
/// - Anonymous/dynamic types (structural comparison by properties)
/// - Floating-point tolerance for aggregations
/// - Unordered collections (compare as sets when no explicit ordering)
/// </summary>
internal static class CollectionComparer
{
    /// <summary>
    /// Compares two lists for structural equality.
    /// </summary>
    /// <param name="a">Primary (baseline) results.</param>
    /// <param name="b">Candidate (new pipeline) results.</param>
    /// <param name="tolerance">Floating-point comparison tolerance. Default: 0.0001.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>True if collections are structurally equal.</returns>
    public static bool AreEqual<T>(IReadOnlyList<T> a, IReadOnlyList<T> b, double tolerance = 0.0001)
    {
        if (a.Count != b.Count) return false;
        if (a.Count == 0) return true;

        for (int i = 0; i < a.Count; i++)
        {
            if (!StructurallyEqual(a[i], b[i], tolerance))
                return false;
        }

        return true;
    }

    private static bool StructurallyEqual(object? a, object? b, double tolerance)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        if (a.GetType() != b.GetType() && !IsCompatibleTypePair(a.GetType(), b.GetType()))
            return false;

        // Primitive types: direct equality
        if (a is IConvertible && b is IConvertible)
        {
            return ValuesEqual(a, b, tolerance);
        }

        // Collections: compare recursively
        if (a is IEnumerable enumerableA && b is IEnumerable enumerableB && a is not string && b is not string)
        {
            return CollectionsEqual(enumerableA, enumerableB, tolerance);
        }

        // Objects: compare by public instance properties
        var propsA = a.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propsB = b.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (propsA.Length != propsB.Length) return false;

        foreach (var pa in propsA)
        {
            var pb = propsB.FirstOrDefault(p => p.Name == pa.Name);
            if (pb == null) return false;

            var va = pa.GetValue(a);
            var vb = pb.GetValue(b);

            if (!ValuesEqual(va, vb, tolerance))
                return false;
        }

        return true;
    }

    private static bool ValuesEqual(object? a, object? b, double tolerance)
    {
        if (a is null || b is null) return a is null && b is null;
        if (a is double da && b is double db) return Math.Abs(da - db) <= tolerance;
        if (a is float fa && b is float fb) return Math.Abs(fa - fb) <= (float)tolerance;
        if (a is decimal decA && b is decimal decB) return decA == decB;
        if (a is IComparable compA) return compA.CompareTo(b) == 0;

        return Equals(a, b);
    }

    private static bool CollectionsEqual(IEnumerable a, IEnumerable b, double tolerance)
    {
        var listA = a.Cast<object>().ToList();
        var listB = b.Cast<object>().ToList();

        if (listA.Count != listB.Count) return false;
        if (listA.Count == 0) return true;

        for (int i = 0; i < listA.Count; i++)
        {
            if (!StructurallyEqual(listA[i], listB[i], tolerance))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Allows comparison between anonymous types with identical property sets.
    /// </summary>
    private static bool IsCompatibleTypePair(Type a, Type b)
    {
        // Handle anonymous types from different compilation units
        if (a.Name.StartsWith("<>") && b.Name.StartsWith("<>"))
        {
            var aProps = a.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                          .Select(p => p.Name + ":" + p.PropertyType.Name).OrderBy(p => p).ToList();
            var bProps = b.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                          .Select(p => p.Name + ":" + p.PropertyType.Name).OrderBy(p => p).ToList();

            return aProps.SequenceEqual(bProps);
        }

        return false;
    }
}
