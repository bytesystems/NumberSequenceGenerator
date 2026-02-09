using System.Reflection;
using Bytesystems.NumberSequenceGenerator.Attributes;

namespace Bytesystems.NumberSequenceGenerator.Services;

/// <summary>
/// Reads <see cref="SequenceAttribute"/> annotations from entity types using reflection.
/// </summary>
public class AnnotationReader
{
    /// <summary>
    /// Finds all properties on the given type that are decorated with <see cref="SequenceAttribute"/>.
    /// </summary>
    /// <returns>Dictionary mapping property names to their SequenceAttribute.</returns>
    public Dictionary<string, SequenceAttribute> GetPropertiesWithSequenceAttribute(Type entityType)
    {
        var result = new Dictionary<string, SequenceAttribute>();

        var properties = entityType.GetProperties(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var attr = property.GetCustomAttribute<SequenceAttribute>();
            if (attr != null)
            {
                result[property.Name] = attr;
            }
        }

        return result;
    }
}
