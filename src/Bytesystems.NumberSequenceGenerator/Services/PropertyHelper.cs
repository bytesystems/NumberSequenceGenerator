using System.Reflection;

namespace Bytesystems.NumberSequenceGenerator.Services;

/// <summary>
/// Reflection helper for reading and writing property values on entity objects.
/// </summary>
public class PropertyHelper
{
    /// <summary>
    /// Gets the value of a property on the given object by name.
    /// </summary>
    public object? GetValue(object entity, string propertyName)
    {
        var type = entity.GetType();
        var property = type.GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (property == null)
            throw new InvalidOperationException(
                $"Property '{propertyName}' not found on type '{type.Name}'.");

        return property.GetValue(entity);
    }

    /// <summary>
    /// Sets the value of a property on the given object by name.
    /// Handles both public and private setters.
    /// </summary>
    public void SetValue(object entity, string propertyName, object? value)
    {
        var type = entity.GetType();
        var property = type.GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (property == null)
            throw new InvalidOperationException(
                $"Property '{propertyName}' not found on type '{type.Name}'.");

        // Try the setter first, then fall back to backing field
        var setter = property.GetSetMethod(nonPublic: true);
        if (setter != null)
        {
            setter.Invoke(entity, [value]);
            return;
        }

        // Try backing field for auto-properties (e.g. <PropertyName>k__BackingField)
        var backingField = type.GetField($"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (backingField != null)
        {
            backingField.SetValue(entity, value);
            return;
        }

        throw new InvalidOperationException(
            $"Cannot set property '{propertyName}' on type '{type.Name}'. No accessible setter or backing field found.");
    }
}
