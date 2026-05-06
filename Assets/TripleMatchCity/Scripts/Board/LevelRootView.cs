using UnityEngine;

namespace TripleMatch.Board
{
    /// <summary>
    /// Sits on the LevelRoot scaffold prefab. Holds direct references to the structural
    /// children that BoardManager populates at runtime: the background SpriteRenderer
    /// and the two item containers (collectible / non-collectible).
    /// </summary>
    public class LevelRootView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Transform collectibleItems;
        [SerializeField] private Transform nonCollectibleItems;

        public SpriteRenderer Background => background;
        public Transform CollectibleItems => collectibleItems;
        public Transform NonCollectibleItems => nonCollectibleItems;
    }
}
