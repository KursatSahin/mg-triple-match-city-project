using System;
using System.Collections.Generic;
using TripleMatch.Data;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace TripleMatch.Board
{
    /// <summary>
    /// Two layer object pool for collectible items.
    /// First layer: a single host pool of generic CollectibleItemView (always alive).
    /// Second layer: per-type visual pools, capped by MaxVisualPoolCount and evicted LRU when
    /// the cap is exceeded. Eviction destroys all pooled visuals of that type; visuals that
    /// were spawned (in flight or on board) at the moment of eviction stay alive but cannot
    /// return to a pool — they are destroyed on despawn.
    /// </summary>
    public class ItemFactory : IItemFactory, IDisposable
    {
        // const int DefaultCapacity = 16;
        // const int MaxPoolSize = 256;
        // const int DefaultMaxVisualPoolCount = 16;

        readonly ItemPoolConfigSO _itemPoolConfigSo;
        readonly CollectibleItemView _genericHostItemPrefab;
        readonly ObjectPool<CollectibleItemView> _genericHostItemPool;
        readonly Dictionary<CollectibleItemData, LinkedListNode<VisualPoolEntry>> _visualItemPoolLookup = new();
        readonly LinkedList<VisualPoolEntry> _visualItemPoolLruOrder = new();
        readonly int _maxVisualItemPoolCount;

        Transform _poolRoot;

        sealed class VisualPoolEntry
        {
            public CollectibleItemData Type;
            public ObjectPool<GameObject> Pool;
        }

        public ItemFactory(CollectibleItemView genericHostItemPrefab, ItemPoolConfigSO itemPoolConfigSo)
        {
            _genericHostItemPrefab = genericHostItemPrefab;
            _itemPoolConfigSo = itemPoolConfigSo;
            
            _maxVisualItemPoolCount = Mathf.Max(1, _itemPoolConfigSo.DefaultMaxVisualPoolCount);

            _genericHostItemPool = new ObjectPool<CollectibleItemView>(
                createFunc: CreateGenericHostItem,
                actionOnGet: null,
                actionOnRelease: OnReleaseGenericHostItem,
                actionOnDestroy: OnDestroyGenericHostItem,
                collectionCheck: true,
                defaultCapacity: _itemPoolConfigSo.DefaultCapacity,
                maxSize: _itemPoolConfigSo.MaxPoolSize);
        }

        public CollectibleItemView Spawn(CollectibleItemInstanceData data, Transform parent, CollectibleItemView parentItem)
        {
            var genericHostItem = _genericHostItemPool.Get();
            genericHostItem.transform.SetParent(parent, worldPositionStays: false);

            var visualPool = GetOrCreateVisualPool(data.Item);
            if (visualPool != null)
            {
                var visual = visualPool.Get();
                genericHostItem.AttachVisual(visual);
            }
            else
            {
                genericHostItem.AttachVisual(null);
            }

            genericHostItem.Initialize(data, parentItem);
            return genericHostItem;
        }

        public void Despawn(CollectibleItemView collectibleItemView)
        {
            if (collectibleItemView == null) return;

            var itemData = collectibleItemView.ItemData;
            var visualItem = collectibleItemView.DetachVisual();

            if (visualItem != null)
            {
                if (itemData != null && _visualItemPoolLookup.TryGetValue(itemData, out var visualPoolEntry))
                {
                    // Touch as MRU; this type just got used.
                    TouchAsMostRecent(visualPoolEntry);
                    visualPoolEntry.Value.Pool.Release(visualItem);
                }
                else
                {
                    // Pool was evicted while this visual was spawned. Destroy it.
                    UnityEngine.Object.Destroy(visualItem);
                }
            }

            _genericHostItemPool.Release(collectibleItemView);
        }

        ObjectPool<GameObject> GetOrCreateVisualPool(CollectibleItemData itemType)
        {
            if (itemType == null) return null;

            if (_visualItemPoolLookup.TryGetValue(itemType, out var existing))
            {
                TouchAsMostRecent(existing);
                return existing.Value.Pool;
            }

            // Make room before creating a new pool.
            while (_visualItemPoolLookup.Count >= _maxVisualItemPoolCount)
            {
                EvictOldestVisualItemPool();
            }

            var prefab = itemType.VisualPrefab;
            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreateVisualItem(prefab),
                actionOnGet: null,
                actionOnRelease: OnReleaseVisualItem,
                actionOnDestroy: OnDestroyVisualItem,
                collectionCheck: true,
                defaultCapacity: _itemPoolConfigSo.DefaultCapacity,
                maxSize: _itemPoolConfigSo.MaxPoolSize);

            var entry = new VisualPoolEntry { Type = itemType, Pool = pool };
            var node = new LinkedListNode<VisualPoolEntry>(entry);
            _visualItemPoolLruOrder.AddLast(node);
            _visualItemPoolLookup[itemType] = node;

            return pool;
        }

        GameObject CreateVisualItem(GameObject visualItemPrefab)
        {
            return UnityEngine.Object.Instantiate(visualItemPrefab);
        }
        
        void TouchAsMostRecent(LinkedListNode<VisualPoolEntry> node)
        {
            if (_visualItemPoolLruOrder.Last == node) return;
            _visualItemPoolLruOrder.Remove(node);
            _visualItemPoolLruOrder.AddLast(node);
        }

        void EvictOldestVisualItemPool()
        {
            var oldest = _visualItemPoolLruOrder.First;
            if (oldest == null) return;

            _visualItemPoolLruOrder.RemoveFirst();
            _visualItemPoolLookup.Remove(oldest.Value.Type);
            oldest.Value.Pool.Dispose();
        }

        CollectibleItemView CreateGenericHostItem()
        {
            return UnityEngine.Object.Instantiate(_genericHostItemPrefab);
        }

        void OnReleaseGenericHostItem(CollectibleItemView view)
        {
            view.ResetForPool();
            CheckOrCreatePoolRoot();
            view.transform.SetParent(_poolRoot, worldPositionStays: false);
        }

        static void OnDestroyGenericHostItem(CollectibleItemView view)
        {
            if (view != null) UnityEngine.Object.Destroy(view.gameObject);
        }

        void OnReleaseVisualItem(GameObject visual)
        {
            if (visual == null) return;
            CheckOrCreatePoolRoot();
            visual.transform.SetParent(_poolRoot, worldPositionStays: false);
        }

        static void OnDestroyVisualItem(GameObject visual)
        {
            if (visual != null) UnityEngine.Object.Destroy(visual);
        }

        void CheckOrCreatePoolRoot()
        {
            if (_poolRoot != null) return;
            var go = new GameObject("[ItemFactoryPool]");
            go.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(go);
            _poolRoot = go.transform;
        }

        public void Dispose()
        {
            _genericHostItemPool.Dispose();

            foreach (var node in _visualItemPoolLookup.Values)
                node.Value.Pool.Dispose();
            _visualItemPoolLookup.Clear();
            _visualItemPoolLruOrder.Clear();

            if (_poolRoot != null)
            {
                UnityEngine.Object.Destroy(_poolRoot.gameObject);
                _poolRoot = null;
            }
        }
    }
}
