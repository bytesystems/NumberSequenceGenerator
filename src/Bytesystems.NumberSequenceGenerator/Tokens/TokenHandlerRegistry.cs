namespace Bytesystems.NumberSequenceGenerator.Tokens;

/// <summary>
/// Registry of all available token handlers. Used by the NumberGenerator to process pattern tokens.
/// </summary>
public class TokenHandlerRegistry
{
    private readonly List<ITokenHandler> _handlers = [];

    public TokenHandlerRegistry(IEnumerable<ITokenHandler> handlers)
    {
        _handlers.AddRange(handlers);
    }

    /// <summary>
    /// Returns all registered token handlers.
    /// </summary>
    public IReadOnlyList<ITokenHandler> Handlers => _handlers;

    /// <summary>
    /// Registers an additional token handler.
    /// </summary>
    public void Register(ITokenHandler handler)
    {
        _handlers.Add(handler);
    }
}
