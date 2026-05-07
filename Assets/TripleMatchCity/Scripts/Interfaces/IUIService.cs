using Cysharp.Threading.Tasks;

namespace TripleMatch.UI
{
    /// <summary>
    /// Single point of control for screen-style UI (popups, modals, full-screen overlays).
    /// Maintains a stack so nested screens (popup over popup) tear down in the right order.
    /// HUD-style ambient UI (Goal panel, Timer panel) is not managed here.
    /// </summary>
    public interface IUIService
    {
        /// <summary>The screen currently on top of the stack, or null if no screen is open.</summary>
        IScreen Top { get; }

        /// <summary>
        /// Resolves the screen instance from the DI container, calls OnOpenAsync, and pushes
        /// it onto the stack. Returns the resolved instance.
        /// </summary>
        UniTask<TScreen> Open<TScreen, TArgs>(TArgs args) where TScreen : class, IScreen<TArgs>;

        /// <summary>Close the screen currently on top of the stack.</summary>
        UniTask CloseTop();

        /// <summary>Close every open screen, top first.</summary>
        UniTask CloseAll();

        /// <summary>True if a screen of the given type is currently open.</summary>
        bool IsOpen<TScreen>() where TScreen : class, IScreen;
    }
}
