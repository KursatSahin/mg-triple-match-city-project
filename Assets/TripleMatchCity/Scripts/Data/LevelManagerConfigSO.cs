using UnityEngine;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Config/Level Manager Config")]
    public class LevelManagerConfigSO : ScriptableObject
    {
        public int LoopStartIndex = 1;
    }
}
