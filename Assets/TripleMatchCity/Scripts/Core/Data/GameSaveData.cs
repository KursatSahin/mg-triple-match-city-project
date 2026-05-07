using System;

namespace TripleMatch.Core.Data
{
    /// <summary>
    /// Root model for all persisted game data. Serialized as a single JSON blob by DataManager.
    /// New fields added later will get C# default values when loading older saves
    /// (Unity's JsonUtility ignores missing fields).
    /// </summary>
    [Serializable]
    public sealed class GameSaveData
    {
        public int CurrentDisplayIndex = 1;
        public int LastCompletedDisplayIndex = 0;
    }
}
