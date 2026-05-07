using TripleMatch.Data;

namespace TripleMatch.Level
{
    public interface ILevelManager
    {
        LevelDataSO CurrentLevel { get; }
        int CurrentDisplayIndex { get; }
        int CurrentActualIndex { get; }
        int LastCompletedDisplayIndex { get; }

        void LoadLevel(int displayIndex);
        void OnLevelCompleted();
        void ResetProgress();
    }
}
