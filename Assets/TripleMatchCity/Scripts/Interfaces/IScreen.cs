using Cysharp.Threading.Tasks;

namespace TripleMatch.UI
{
    /// <summary>
    /// Base marker interface for any screen managed by IUIService.
    /// Carries the close path and the input-blocking flag so the service can stack and tear
    /// down screens polymorphically without knowing the args type.
    /// </summary>
    public interface IScreen
    {
        /// <summary>True if underlying input/HUD should be blocked while this screen is on top.</summary>
        bool BlockInput { get; }

        /// <summary>Close handler. Async to allow exit transitions.</summary>
        UniTask OnCloseAsync();
    }

    /// <summary>
    /// Typed screen contract. The args type is part of the screen identity so callers cannot
    /// pass the wrong shape at compile time.
    /// </summary>
    public interface IScreen<TArgs> : IScreen
    {
        /// <summary>Open handler. Async to allow entrance transitions.</summary>
        UniTask OnOpenAsync(TArgs args);
    }
}
