using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Board
{
    public interface IItemFactory
    {
        CollectibleItemView Spawn(
            CollectibleItemInstanceData data,
            Transform parent,
            CollectibleItemView parentItem);

        void Despawn(CollectibleItemView view);
    }
}
