using Bytesystems.NumberSequenceGenerator.Interceptors;
using Bytesystems.NumberSequenceGenerator.Services;
using Bytesystems.NumberSequenceGenerator.Tokens;
using Bytesystems.NumberSequenceGenerator.Tokens.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Bytesystems.NumberSequenceGenerator.Extensions;

/// <summary>
/// Extension methods for registering NumberSequenceGenerator services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all NumberSequenceGenerator services including the EF Core interceptor,
    /// built-in token handlers, and supporting services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action for additional token handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddNumberSequenceGenerator();
    ///
    /// // With custom token handler:
    /// builder.Services.AddNumberSequenceGenerator(options =>
    /// {
    ///     options.AddTokenHandler&lt;MyCustomTokenHandler&gt;();
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddNumberSequenceGenerator(
        this IServiceCollection services,
        Action<NumberSequenceGeneratorOptions>? configure = null)
    {
        var options = new NumberSequenceGeneratorOptions();
        configure?.Invoke(options);

        // Register built-in token handlers
        services.AddSingleton<ITokenHandler, SequenceTokenHandler>();
        services.AddSingleton<ITokenHandler, DateTokenHandler>();
        services.AddSingleton<ITokenHandler, WeekTokenHandler>();

        // Register custom token handlers
        foreach (var handlerType in options.CustomTokenHandlers)
        {
            services.AddSingleton(typeof(ITokenHandler), handlerType);
        }

        // Register core services
        services.AddSingleton<TokenHandlerRegistry>(sp =>
            new TokenHandlerRegistry(sp.GetServices<ITokenHandler>()));

        services.AddSingleton<AnnotationReader>();
        services.AddSingleton<PropertyHelper>();
        services.AddSingleton<SegmentResolver>();
        services.AddScoped<NumberGenerator>();
        services.AddScoped<NumberSequenceInterceptor>();

        return services;
    }
}

/// <summary>
/// Configuration options for the NumberSequenceGenerator.
/// </summary>
public class NumberSequenceGeneratorOptions
{
    internal List<Type> CustomTokenHandlers { get; } = [];

    /// <summary>
    /// Registers a custom token handler type.
    /// </summary>
    /// <typeparam name="THandler">The token handler type implementing <see cref="ITokenHandler"/>.</typeparam>
    public NumberSequenceGeneratorOptions AddTokenHandler<THandler>() where THandler : class, ITokenHandler
    {
        CustomTokenHandlers.Add(typeof(THandler));
        return this;
    }
}
