namespace TripleMatch.StateMachine
{
    public enum GameState
    {
        Playing,
        Won,
        Failed
    }

    public interface IGameStateMachine
    {
        GameState CurrentState { get; }
        bool IsPlaying { get; }
    }
}
