using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Dsr.Architecture.Utilities;

/// <summary>
/// Provides a collection of utility methods for various operations.
/// </summary>
public static class Utilities
{
    private static readonly Random dsrRandom = new();

    /// <summary>
    /// Gets or sets the culture information used for formatting.
    /// </summary>
    public static CultureInfo DsrCulture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Gets the text information based on the current culture.
    /// </summary>
    public static TextInfo DsrTextInfo { get { return DsrCulture?.TextInfo ?? CultureInfo.InstalledUICulture.TextInfo; } }

    /// <summary>
    /// JSON settings to ignore circular references and format with indentation.
    /// </summary>
    private static readonly JsonSerializerSettings _settings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.Indented
    };

    /// <summary>
    /// Determines whether a type is a native .NET type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is native; otherwise, false.</returns>
    public static bool IsNative(this Type type)
        => type.IsValueType || type.IsPrimitive || type.Name.ToLower() switch
        {
            "string" => true,
            "decimal" => true,
            "double" => true,
            "int16" => true,
            "int32" => true,
            "int64" => true,
            "datetime" => true,
            "boolean" => true,
            "guid" => true,
            "objectid" => true,
            _ => type == typeof(double?) || type == typeof(short?) || type == typeof(int?) || type == typeof(long?) ||
                 type == typeof(decimal?) || type == typeof(DateTime?) || type == typeof(bool?) ||
                 type == typeof(string) || type == typeof(Guid) || type == typeof(ObjectId)
        };

    /// <summary>
    /// Determines whether a property is of a native .NET type and is writable.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <returns>True if the property is native and writable; otherwise, false.</returns>
    public static bool IsNative(this PropertyInfo property)
        => property.CanWrite && IsNative(property.PropertyType);

    /// <summary>
    /// Determines whether an object is a numeric type.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object is numeric; otherwise, false.</returns>
    public static bool IsNumeric(this object obj)
    {
        if (obj is null || obj is DateTime)
            return false;

        if (obj is short || obj is int || obj is long || obj is decimal || obj is Single || obj is double)
            return true;

        try
        {
            if (obj is string)
                _ = double.Parse(obj as string ?? string.Empty);
            else
                _ = double.Parse(obj.ToString() ?? string.Empty);
            return true;
        }
        catch { }
        return false;
    }

    /// <summary>
    /// Determines whether a type is a generic collection.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a generic collection; otherwise, false.</returns>
    public static bool IsCollection(this Type type)
        => type.GenericTypeArguments.FirstOrDefault() != null;

    /// <summary>
    /// Creates a new instance of a type.
    /// </summary>
    /// <param name="t">The type to instantiate.</param>
    /// <returns>A new instance of the type, or null if instantiation fails.</returns>
    public static object? GetNewInstance(this Type t)
    {
        try
        {
            if (t is null)
                return null;
            if (t.IsAbstract || t.IsInterface)
                return null;

            return Activator.CreateInstance(t);
        }
        catch
        {
            try
            {
                var arg = t.GenericTypeArguments?.FirstOrDefault();
                if (arg != null && t.GenericTypeArguments.Length == 0)
                    return Activator.CreateInstance(t.GenericTypeArguments.FirstOrDefault());
                else
                    return Activator.CreateInstance(t);
            }
            catch
            {
                if (t.Name == "String")
                    return string.Empty;
                return null;
            }
        }
    }

    /// <summary>
    /// Creates a shallow clone of an object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to clone.</param>
    /// <returns>A new instance of the object with the same property values.</returns>
    public static T? Clone<T>(this T obj) where T : class
    {
        if (obj is null)
            return null;

        var type = obj.GetType();
        if (IsNative(type))
            return obj;

        var clone = Activator.CreateInstance(type);
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanWrite)
            {
                var value = prop.GetValue(obj);
                prop.SetValue(clone, value);
            }
        }

        return clone as T;
    }

    /// <summary>
    /// Creates a shallow clone of an object.
    /// </summary>
    /// <param name="obj">The object to clone.</param>
    /// <returns>A new instance of the object with the same property values.</returns>
    public static object? Clone(this object obj)
    {
        if (obj is null)
            return null;

        var type = obj.GetType();
        if (IsNative(type))
            return obj;

        var clone = Activator.CreateInstance(type);
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanWrite)
            {
                var value = prop.GetValue(obj);
                prop.SetValue(clone, value);
            }
        }

        return clone;
    }

    /// <summary>
    /// Clones an object and parses its ID property to a new type.
    /// </summary>
    /// <typeparam name="TIn">The input object type.</typeparam>
    /// <typeparam name="TOut">The output object type.</typeparam>
    /// <param name="obj">The object to clone.</param>
    /// <returns>A new object of type TOut with parsed ID, or null.</returns>
    public static TOut? CloneParseId<TIn, TOut>(this TIn obj)
        where TIn : class
        where TOut : class
    {
        if (obj is null)
            return null;

        var typeIn = obj.GetType();
        var typeOut = typeof(TOut);

        if (IsNative(typeIn) || IsNative(typeOut))
            return null;

        var clone = Activator.CreateInstance(typeOut);
        var propertiesIn = typeIn.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propertiesOut = typeOut.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var idPropertyIn = typeIn.GetProperty("Id");
        var idPropertyOut = typeOut.GetProperty("Id");

        foreach (var propOut in propertiesOut)
        {
            if (!propOut.CanWrite) continue;

            var propIn = propertiesIn.FirstOrDefault(p => p.Name == propOut.Name);
            if (propIn is null) continue;

            if (IsNative(propOut.PropertyType))
            {
                if (propOut.Name == "Id" && idPropertyIn != null && idPropertyOut != null)
                {
                    var idValue = idPropertyIn.GetValue(obj)?.ToString();
                    if (idValue != null)
                    {
                        var newId = propOut.PropertyType.Get(idValue);
                        propOut.SetValue(clone, newId);
                        continue;
                    }
                }
                else
                {
                    var value = propIn.GetValue(obj);
                    propOut.SetValue(clone, value);
                }
            }
            else
            {
                try
                {
                    if (IsCollection(propOut.PropertyType))
                    {
                        dynamic? list = propIn.GetValue(obj);
                        dynamic? listOut = Activator.CreateInstance(propOut.PropertyType);

                        foreach (var item in list ?? Enumerable.Empty<object>())
                        {
                            if (item is null)
                                continue;

                            var subObj = CloneParseId(propIn.PropertyType, propOut.PropertyType, item);

                            if (subObj != null)
                                listOut.Add(subObj);
                        }
                        propOut.SetValue(clone, listOut);
                    }
                    else
                    {
                        var value = propIn.GetValue(obj);
                        if (value is not null)
                        {
                            var subObj = CloneParseId(propIn.PropertyType, propOut.PropertyType, value);
                            propOut.SetValue(clone, subObj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        return clone as TOut;
    }

    /// <summary>
    /// Clones an object and parses its ID property to a new type.
    /// </summary>
    /// <param name="typeIn">The input object type.</param>
    /// <param name="typeOut">The output object type.</param>
    /// <param name="obj">The object to clone.</param>
    /// <returns>A new object with a parsed ID, or null.</returns>
    public static dynamic? CloneParseId(this Type typeIn, Type typeOut, object? obj)
    {
        if (obj is null)
            return null;

        if (IsNative(typeIn) || IsNative(typeOut))
            return null;

        var clone = Activator.CreateInstance(typeOut);
        var propertiesIn = typeIn.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propertiesOut = typeOut.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var idPropertyIn = typeIn.GetProperty("Id");
        var idPropertyOut = typeOut.GetProperty("Id");

        foreach (var propOut in propertiesOut)
        {
            if (!propOut.CanWrite) continue;

            var propIn = propertiesIn.FirstOrDefault(p => p.Name == propOut.Name);
            if (propIn is null) continue;

            if (IsNative(propOut.PropertyType))
            {
                if (propOut.Name == "Id" && idPropertyIn != null && idPropertyOut != null)
                {
                    var idValue = idPropertyIn.GetValue(obj)?.ToString();
                    if (idValue != null)
                    {
                        var newId = propOut.PropertyType.Get(idValue);
                        propOut.SetValue(clone, newId);
                        continue;
                    }
                }
                else
                {
                    var value = propIn.GetValue(obj);
                    propOut.SetValue(clone, value);
                }
            }
            else
            {
                try
                {
                    if (IsCollection(propOut.PropertyType))
                    {
                        dynamic? list = propIn.GetValue(obj);
                        dynamic? listOut = Activator.CreateInstance(propOut.PropertyType);

                        foreach (var item in list ?? Enumerable.Empty<object>())
                        {
                            if (item is null)
                                continue;

                            var subObj = CloneParseId(propIn.PropertyType, propOut.PropertyType, item);

                            if (subObj != null)
                                listOut.Add(subObj);
                        }
                        propOut.SetValue(clone, listOut);
                    }
                    else
                    {
                        var value = propIn.GetValue(obj);
                        if (value is not null)
                        {
                            var subObj = CloneParseId(propIn.PropertyType, propOut.PropertyType, value);
                            propOut.SetValue(clone, subObj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        return clone;
    }

    /// <summary>
    /// Creates multiple instances of an object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to clone.</param>
    /// <param name="instances">The number of instances to create.</param>
    /// <returns>An enumerable of the new instances.</returns>
    public static IEnumerable<T> GetMultiInstances<T>(this T obj, int instances) where T : class
    {
        if (obj is null || instances <= 0)
            return [];

        if (IsNative(obj.GetType()))
            return Enumerable.Repeat(obj, instances);

        var clone = obj.Clone() ?? Activator.CreateInstance<T>();

        return Enumerable.Repeat(clone, instances);
    }

    /// <summary>
    /// Gets a random element from an array.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="array">The array to select from.</param>
    /// <returns>A random element from the array.</returns>
    public static T GetRandom<T>(this T[] array)
        => array[dsrRandom.Next(array.Length)];

    /// <summary>
    /// Gets a random element from a list.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="list">The list to select from.</param>
    /// <returns>A random element from the list.</returns>
    public static T GetRandom<T>(this IList<T> list)
        => list[dsrRandom.Next(list.Count)];

    /// <summary>
    /// Gets a random element from an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="list">The enumerable to select from.</param>
    /// <returns>A random element from the enumerable.</returns>
    public static T GetRandom<T>(this IEnumerable<T> list)
        => list.ElementAt(dsrRandom.Next(list.Count()));

    /// <summary>
    /// Gets a specified number of unique random elements from an array.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="list">The array to select from.</param>
    /// <param name="count">The number of elements to select.</param>
    /// <returns>An array of unique random elements.</returns>
    public static T[] GetSelectedRandom<T>(this T[] list, int count)
    {
        var rList = new T[count];
        var oldList = list.ToList();
        var i = 0;
        while (count > 0 && list.Length > 0 && list.Length >= count)
        {
            var n = dsrRandom.Next(0, oldList.Count);
            var e = oldList[n];
            rList[i] = e;
            oldList.RemoveAt(n);
            count--;
            i++;
        }

        return rList;
    }

    /// <summary>
    /// Gets a specified number of unique random elements from a list.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="list">The list to select from.</param>
    /// <param name="count">The number of elements to select.</param>
    /// <returns>A list of unique random elements.</returns>
    public static IList<T> GetSelectedRandom<T>(this IList<T> list, int count)
    {
        var rList = new List<T>();
        var oldList = list;
        while (count > 0 && list.Count > 0 && list.Count >= count)
        {
            var n = dsrRandom.Next(0, oldList.Count);
            var e = oldList[n];
            rList.Add(e);
            oldList.RemoveAt(n);
            count--;
        }

        return rList;
    }

    /// <summary>
    /// Gets a specified number of unique random elements from an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="list">The enumerable to select from.</param>
    /// <param name="count">The number of elements to select.</param>
    /// <returns>An enumerable of unique random elements.</returns>
    public static IEnumerable<T> GetSelectedRandom<T>(this IEnumerable<T> list, int count)
    {
        var rList = new List<T>();
        var oldList = list.ToList();
        while (count > 0 && oldList.Count > 0 && oldList.Count >= count)
        {
            var n = dsrRandom.Next(0, oldList.Count);
            var e = oldList[n];
            rList.Add(e);
            oldList.RemoveAt(n);
            count--;
        }

        return rList;
    }

    /// <summary>
    /// Executes an action for each item in an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="items">The enumerable of items.</param>
    /// <param name="action">The action to execute.</param>
    public static void Each<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes an action for each item in an array.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="items">The array of items.</param>
    /// <param name="action">The action to execute.</param>
    public static void Each<T>(this T[] items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    /// <summary>
    /// Takes the first N characters of a string.
    /// </summary>
    /// <param name="value">The string to process.</param>
    /// <param name="count">The number of characters to take.</param>
    /// <returns>The first N characters of the string.</returns>
    public static string TakeFirst(this string value, int count)
        => ((value?.Length ?? 0) < count ? value : value?[..count]) ?? string.Empty;

    /// <summary>
    /// Converts an object to a specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="value">The object to convert.</param>
    /// <returns>The converted object, or null if conversion fails.</returns>
    public static T? Get<T>(this object value)
        => Get(typeof(T), value);

    /// <summary>
    /// Converts an object to a specified type.
    /// </summary>
    /// <param name="type">The target type.</param>
    /// <param name="value">The object to convert.</param>
    /// <returns>The converted object, or null if conversion fails.</returns>
    public static dynamic? Get(this Type type, object value)
    {
        if (value is null) return null;

        switch (type.Name)
        {
            case "String":
            case "Decimal":
            case "Int16":
            case "Int32":
            case "Int64":
            case "DateTime":
            case "Double":
                if (value != DBNull.Value)
                    return Convert.ChangeType(value, type);
                break;

            case "Boolean":
                if (value != DBNull.Value)
                    return GetBoolean(value);
                break;

            case "Guid":
                if (value != DBNull.Value)
                    return Guid.Parse(value.ToString() ?? string.Empty);
                break;

            case "ObjectId":
                if (value != DBNull.Value)
                    return ObjectId.Parse(value.ToString() ?? string.Empty);
                break;

            default:
                if (type == typeof(int?) || type == typeof(bool?) || type == typeof(short?) || type == typeof(long?) || type == typeof(double?) || type == typeof(decimal?) || type == typeof(DateTime?))
                {
                    var t = Nullable.GetUnderlyingType(type) ?? type;
                    var safeValue = (value is null || value == DBNull.Value) ? null : Convert.ChangeType(value, t);
                    return safeValue;
                }

                break;
        }

        return null;
    }

    /// <summary>
    /// Converts an object to a string based on an enum type.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <param name="type">The enum type specifying the conversion format.</param>
    /// <returns>The string representation of the object, or null.</returns>
    public static string? GetStringValueByEnumType(this object value, Enums.Types type)
    {
        if (value is null) return null;

        switch (type)
        {
            case Enums.Types.String:
            case Enums.Types.Int16:
            case Enums.Types.Int32:
            case Enums.Types.Int64:
            case Enums.Types.Decimal:
            case Enums.Types.Double:
            case Enums.Types.Float:
            case Enums.Types.DateTime:
                if (value != DBNull.Value)
                    return Convert.ChangeType(value, Type.GetType($"System.{type.GetType()?.GetEnumName(type)}") ?? typeof(string)).ToString();
                break;

            case Enums.Types.Boolean:
                if (value != DBNull.Value)
                    return GetBoolean(value).ToString();
                break;

            case Enums.Types.Money:
                if (value != DBNull.Value)
                    return Convert.ToDecimal(value).ToString("C2", DsrCulture);
                break;

            case Enums.Types.FullDateTime:
                if (value != DBNull.Value)
                    return Convert.ToDateTime(value).ToString(DsrCulture.DateTimeFormat.FullDateTimePattern, DsrCulture);
                break;

            case Enums.Types.SortableDateTime:
                if (value != DBNull.Value)
                    return Convert.ToDateTime(value).ToString(DsrCulture.DateTimeFormat.SortableDateTimePattern, DsrCulture);
                break;

            case Enums.Types.LongDate:
                if (value != DBNull.Value)
                    return Convert.ToDateTime(value).ToString(DsrCulture.DateTimeFormat.LongDatePattern, DsrCulture);
                break;

            case Enums.Types.ShortDate:
                if (value != DBNull.Value)
                    return Convert.ToDateTime(value).ToString(DsrCulture.DateTimeFormat.ShortDatePattern, DsrCulture);
                break;

            case Enums.Types.LongTime:
                if (value != DBNull.Value)
                    return Convert.ToDateTime(value).ToString(DsrCulture.DateTimeFormat.LongTimePattern, DsrCulture);
                break;

            case Enums.Types.ShortTime:
                if (value != DBNull.Value)
                    return Convert.ToDateTime(value).ToString(DsrCulture.DateTimeFormat.ShortTimePattern, DsrCulture);
                break;
            default:
                return value.ToString();
        }

        return null;
    }

    /// <summary>
    /// Converts an object to a dynamic value based on an enum type.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <param name="type">The enum type specifying the conversion format.</param>
    /// <returns>The converted value, or null.</returns>
    public static dynamic? GetValueByEnumType(this object value, Enums.Types type)
    {
        if (value is null) return null;

        switch (type)
        {
            case Enums.Types.String:
            case Enums.Types.Int16:
            case Enums.Types.Int32:
            case Enums.Types.Int64:
            case Enums.Types.Decimal:
            case Enums.Types.Double:
            case Enums.Types.Float:
            case Enums.Types.DateTime:
                if (value != DBNull.Value)
                    return Convert.ChangeType(value, Type.GetType($"System.{type.GetType()?.GetEnumName(type)}") ?? typeof(string));
                break;

            case Enums.Types.Boolean:
                if (value != DBNull.Value)
                    return GetBoolean(value);
                break;

            case Enums.Types.Money:
                if (value != DBNull.Value)
                    return Convert.ToDecimal(value);
                break;

            case Enums.Types.FullDateTime:
            case Enums.Types.SortableDateTime:
            case Enums.Types.LongDate:
            case Enums.Types.ShortDate:
            case Enums.Types.ShortTime:
            case Enums.Types.LongTime:
                if (value != DBNull.Value)
                    return Convert.ToDateTime(value);
                break;
            default:
                var _type = value.GetType();
                if (_type == typeof(short?) || _type == typeof(int?) || _type == typeof(long?) || _type == typeof(double?) ||
                    _type == typeof(decimal?) || _type == typeof(DateTime?) || _type == typeof(bool?))
                {
                    var t = Nullable.GetUnderlyingType(_type) ?? _type;
                    var safeValue = (value is null || value == DBNull.Value) ? null : Convert.ChangeType(value, t);
                    return safeValue;
                }

                break;
        }

        return null;
    }

    /// <summary>
    /// Converts a string to a nullable value type.
    /// </summary>
    /// <typeparam name="T">The target value type.</typeparam>
    /// <param name="valueAsString">The string to convert.</param>
    /// <returns>The converted value, or null if the string is empty.</returns>
    public static T? GetValueOrNull<T>(this string valueAsString) where T : struct
    {
        if (string.IsNullOrEmpty(valueAsString))
            return null;
        return (T)Convert.ChangeType(valueAsString, typeof(T));
    }

    /// <summary>
    /// Converts an object to a string.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The string representation of the object, or null.</returns>
    public static string? GetString(this object value)
        => value is not null && value != DBNull.Value ? value.ToString() : null;

    /// <summary>
    /// Converts an object to a nullable double.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The converted double, or null.</returns>
    public static double? GetDouble(this object value)
        => value is not null && value != DBNull.Value ? Convert.ToDouble(value) : null;

    /// <summary>
    /// Converts an object to a nullable decimal.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The converted decimal, or null.</returns>
    public static decimal? GetDecimal(this object value)
        => value is not null && value != DBNull.Value ? Convert.ToDecimal(value) : null;

    /// <summary>
    /// Converts an object to a nullable long.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The converted long, or null.</returns>
    public static long? GetInt64(this object value)
        => value is not null && value != DBNull.Value ? Convert.ToInt64(value) : null;

    /// <summary>
    /// Converts an object to a nullable int.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The converted int, or null.</returns>
    public static int? GetInt32(this object value)
        => value is not null && value != DBNull.Value ? Convert.ToInt32(value) : null;

    /// <summary>
    /// Converts an object to a nullable short.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The converted short, or null.</returns>
    public static short? GetInt16(this object value)
        => value is not null && value != DBNull.Value ? Convert.ToInt16(value) : null;

    /// <summary>
    /// Converts an object to a nullable DateTime.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The converted DateTime, or null.</returns>
    public static DateTime? GetDateTime(this object value)
        => value is not null && value != DBNull.Value ? Convert.ToDateTime(value) : null;

    /// <summary>
    /// Converts an object to a nullable boolean.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The converted boolean, or null.</returns>
    private static bool? GetBoolean(object value)
    {
        if (value is null || value == DBNull.Value) return null;
        if (value is bool v) return v;

        var strValue = value.ToString()?.ToLower().Trim();
        if (strValue == "true" || strValue == "1")
            return true;
        if (strValue == "false" || strValue == "0")
            return false;

        return null;
    }

    /// <summary>
    /// Converts a string to title case.
    /// </summary>
    /// <param name="strText">The string to convert.</param>
    /// <returns>The converted string in title case.</returns>
    public static string ToTitleCase(this string strText)
        => string.IsNullOrEmpty(strText) ? strText : DsrTextInfo.ToTitleCase(strText.ToLower());

    /// <summary>
    /// Converts a string to camel case.
    /// </summary>
    /// <param name="strText">The string to convert.</param>
    /// <returns>The converted string in camel case.</returns>
    public static string ToCamelCase(this string strText)
        => string.IsNullOrEmpty(strText) ? strText : DsrTextInfo.ToLower(strText[0]) + strText[1..];


    /// <summary>
    /// Serializes an object to JSON using the specified settings.
    /// </summary>
    /// <param name="obj">Object to serialize.</param>
    /// <param name="settings">Optional JSON serialization settings.</param>
    /// <returns>Serialized JSON string.</returns>
    public static string JsonSerialize(this object obj, JsonSerializerSettings? settings = null)
        => JsonConvert.SerializeObject(obj, settings ?? _settings);

    /// <summary>
    /// Deserializes a JSON string to a simple entity of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="obj">JSON string to deserialize.</param>
    /// <returns>Deserialized entity.</returns>
    public static T? ToEntitySimple<T>(this string? obj)
        => obj != null ? JsonConvert.DeserializeObject<T>(obj) : default;

    /// <summary>
    /// Deserializes a JSON string to an entity of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="obj">JSON string to deserialize.</param>
    /// <returns>Deserialized entity.</returns>
    public static T ToEntity<T>(this string obj) where T : class, new()
    {
        T? returnObj = ToEntitySimple<T>(obj);

        if (returnObj != null) return returnObj;

        returnObj = new();
        PropertyInfo[] piProperties = typeof(T).GetProperties();
        JObject jObj = JObject.Parse(obj);

        foreach (PropertyInfo piProperty in piProperties)
        {
            try
            {
                string? stringValue = (string?)jObj.SelectToken(piProperty.Name);
                if (stringValue != null)
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(piProperty.PropertyType);
                    piProperty.SetValue(returnObj, tc.ConvertFromString(null, CultureInfo.InvariantCulture, stringValue));
                }
            }
            catch { }
        }

        return returnObj;
    }

    /// <summary>
    /// Deserializes a JSON string to a list of entities of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <typeparam name="D">Type of the sub-entity for nested lists.</typeparam>
    /// <param name="objs">JSON string to deserialize.</param>
    /// <returns>List of deserialized entities.</returns>
    public static List<T> ToEntityList<T, D>(this string objs)
        where T : class, new()
        where D : class, new()
    {
        PropertyInfo[] piProperties = typeof(T).GetProperties();
        List<T> returnObjList = [];
        var jArray = JsonConvert.DeserializeObject<dynamic[]>(objs);

        if (jArray != null)
        {
            foreach (var obj in jArray)
            {
                if (obj != null)
                {
                    JObject jObj = JObject.Parse(obj.ToString());
                    T preReturnObj = new();
                    foreach (PropertyInfo piProperty in piProperties)
                    {
                        if (!typeof(System.Collections.IList).IsAssignableFrom(piProperty.PropertyType))
                        {
                            string? stringValue = (string?)jObj.SelectToken(piProperty.Name);
                            if (stringValue != null)
                            {
                                TypeConverter tc = TypeDescriptor.GetConverter(piProperty.PropertyType);
                                piProperty.SetValue(preReturnObj, tc.ConvertFromString(null, CultureInfo.InvariantCulture, stringValue));
                            }
                        }
                        else if (!typeof(T).Equals(typeof(D)))
                        {
                            if (!string.IsNullOrWhiteSpace(jObj.SelectToken(piProperty.Name)?.ToString()))
                            {
                                JArray jArraySub = JArray.Parse(jObj.SelectToken(piProperty.Name)?.ToString() ?? string.Empty);
                                List<D> asingObjSub = ToEntityList<D, D>(jArraySub.ToString());
                                piProperty.SetValue(preReturnObj, asingObjSub);
                            }
                        }
                    }
                    returnObjList.Add(preReturnObj);
                }
            }
        }
        return returnObjList;
    }

    /// <summary>
    /// Deserializes a JSON string to a list of simple entities of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="objs">JSON string to deserialize.</param>
    /// <returns>List of deserialized entities.</returns>
    public static List<T>? ToEntityListSimple<T>(this string? objs)
        => objs != null ? JsonConvert.DeserializeObject<List<T>>(objs) : null;

    /// <summary>
    /// Deserializes a JSON string to a dictionary.
    /// </summary>
    /// <typeparam name="T">Type of the key.</typeparam>
    /// <typeparam name="D">Type of the value.</typeparam>
    /// <param name="objs">JSON string to deserialize.</param>
    /// <returns>Deserialized dictionary.</returns>
    public static Dictionary<T, D>? ToDictionary<T, D>(this string objs) where T : struct
        => JsonConvert.DeserializeObject<Dictionary<T, D>>(objs);

    /// <summary>
    /// Tries to parse a JSON string to an object of type T.
    /// </summary>
    /// <typeparam name="T">Type of the object.</typeparam>
    /// <param name="json">JSON string to parse.</param>
    /// <param name="result">Parsed object, if successful.</param>
    /// <returns>True if parsing was successful, otherwise false.</returns>
    public static bool TryParseJson<T>(this string json, out T? result)
    {
        bool success = true;

        if (string.IsNullOrEmpty(json))
        {
            success = false;
            result = default;
            return success;
        }

        var settings = new JsonSerializerSettings
        {
            Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
            MissingMemberHandling = MissingMemberHandling.Error
        };

        result = JsonConvert.DeserializeObject<T>(json, settings);
        return success;
    }

}
