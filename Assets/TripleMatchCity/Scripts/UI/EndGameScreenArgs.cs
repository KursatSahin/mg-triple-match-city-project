namespace TripleMatch.UI
{
    /// <summary>
    /// Args for EndGamePopupView. The screen reads the title and star count, then drives its
    /// own UI; the return-home button raises MainMenuRequestedEvent through IEventBus.
    /// </summary>
    public sealed class EndGameScreenArgs
    {
        public string Title;
        public int StarCount;
    }
}
