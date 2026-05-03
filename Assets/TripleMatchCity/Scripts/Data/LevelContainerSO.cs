using System.Collections.Generic;
using UnityEngine;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Config/Level Container")]
    public class LevelContainerSO : ScriptableObject
    {
        public List<LevelDataSO> Levels = new();
    }
}
