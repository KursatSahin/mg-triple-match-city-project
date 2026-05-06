using UnityEngine;

namespace TripleMatch.Board
{
    /// <summary>
    /// Simple level root view component.
    /// Provides references to the background and the parent transforms for collectible and non-collectible items.
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
