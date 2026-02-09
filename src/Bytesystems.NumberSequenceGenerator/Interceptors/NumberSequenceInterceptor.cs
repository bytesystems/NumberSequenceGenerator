using Bytesystems.NumberSequenceGenerator.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bytesystems.NumberSequenceGenerator.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that automatically generates sequence numbers
/// for newly added entities with properties decorated with <see cref="Attributes.SequenceAttribute"/>.
/// This is the C# equivalent of the PHP Doctrine prePersist event subscriber.
/// </summary>
public class NumberSequenceInterceptor : SaveChangesInterceptor
{
    private readonly AnnotationReader _annotationReader;
    private readonly NumberGenerator _numberGenerator;
    private readonly SegmentResolver _segmentResolver;
    private readonly PropertyHelper _propertyHelper;

    public NumberSequenceInterceptor(
        AnnotationReader annotationReader,
        NumberGenerator numberGenerator,
        SegmentResolver segmentResolver,
        PropertyHelper propertyHelper)
    {
        _annotationReader = annotationReader;
        _numberGenerator = numberGenerator;
        _segmentResolver = segmentResolver;
        _propertyHelper = propertyHelper;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var context = eventData.Context;
        var addedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in addedEntries)
        {
            var entity = entry.Entity;
            var entityType = entity.GetType();
            var annotations = _annotationReader.GetPropertiesWithSequenceAttribute(entityType);

            if (annotations.Count == 0)
                continue;

            foreach (var (propertyName, attribute) in annotations)
            {
                // Skip if the property already has a value (don't overwrite manual values)
                var existingValue = _propertyHelper.GetValue(entity, propertyName);
                if (existingValue is string s && !string.IsNullOrEmpty(s))
                    continue;

                var segmentValue = _segmentResolver.ResolveSegmentValue(entity, attribute);
                var segment = _segmentResolver.ResolveSegment(attribute, segmentValue);

                var nextNumber = await _numberGenerator.GetNextNumberAsync(
                    context, attribute, segmentValue, segment);

                _propertyHelper.SetValue(entity, propertyName, nextNumber);
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        // Synchronous fallback -- delegates to async implementation
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);

        var context = eventData.Context;
        var addedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in addedEntries)
        {
            var entity = entry.Entity;
            var entityType = entity.GetType();
            var annotations = _annotationReader.GetPropertiesWithSequenceAttribute(entityType);

            if (annotations.Count == 0)
                continue;

            foreach (var (propertyName, attribute) in annotations)
            {
                var existingValue = _propertyHelper.GetValue(entity, propertyName);
                if (existingValue is string s && !string.IsNullOrEmpty(s))
                    continue;

                var segmentValue = _segmentResolver.ResolveSegmentValue(entity, attribute);
                var segment = _segmentResolver.ResolveSegment(attribute, segmentValue);

                var nextNumber = _numberGenerator.GetNextNumberAsync(
                    context, attribute, segmentValue, segment).GetAwaiter().GetResult();

                _propertyHelper.SetValue(entity, propertyName, nextNumber);
            }
        }

        return base.SavingChanges(eventData, result);
    }
}
