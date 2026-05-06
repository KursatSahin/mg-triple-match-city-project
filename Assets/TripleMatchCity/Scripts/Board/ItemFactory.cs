using System;
using System.Collections.Generic;
using TripleMatch.Data;
using UnityEngine;
using UnityEngine.Pool;

namespace TripleMatch.Board
{
    /// <summary>
    /// Two layer object pool for collectible items.
    /// First layer responsible from generic CollectibleItemView's
    /// Second layer responsible from each item visual prefabs
    /// </summary>
    public class ItemFactory : IItemFactory, IDisposable
    {
        const int DefaultCapacity = 16;
        const int MaxPoolSize = 128;

        readonly CollectibleItemView _hostPrefab;
        readonly ObjectPool<CollectibleItemView> _hostPool;
        readonly Dictionary<CollectibleItemData, ObjectPool<GameObject>> _visualPools = new();
        Transform _poolRoot;

        public ItemFactory(CollectibleItemView hostPrefab)
        {
            _hostPrefab = hostPrefab;
            _hostPool = new ObjectPool<CollectibleItemView>(
                createFunc: CreateHost,
                actionOnGet: null,
                actionOnRelease: OnReleaseHost,
                actionOnDestroy: OnDestroyHost,
                collectionCheck: true,
                defaultCapacity: DefaultCapacity,
                maxSize: MaxPoolSize);
        }

        public CollectibleItemView Spawn(
            CollectibleItemInstanceData data,
            Transform parent,
            CollectibleItemView parentItem)
        {
            var host = _hostPool.Get();
            host.transform.SetParent(parent, worldPositionStays: false);

            var visualPool = GetOrCreateVisualPool(data.Item);
            var visual = visualPool.Get();
            host.AttachVisual(visual);

            host.Initialize(data, parentItem);
            return host;
        }

        public void Despawn(CollectibleItemView view)
        {
            if (view == null) return;

            var itemData = view.ItemData;
            var visual = view.DetachVisual();

            if (visual != null)
            {
                if (itemData != null && _visualPools.TryGetValue(itemData, out var visualPool))
                    visualPool.Release(visual);
                else
                    UnityEngine.Object.Destroy(visual);
            }

            _hostPool.Release(view);
        }

        ObjectPool<GameObject> GetOrCreateVisualPool(CollectibleItemData type)
        {
            if (type == null) return null;
            if (_visualPools.TryGetValue(type, out var existing)) return existing;

            var prefab = type.VisualPrefab;
            var pool = new ObjectPool<GameObject>(
                createFunc: () => UnityEngine.Object.Instantiate(prefab),
                actionOnGet: null,
                actionOnRelease: OnReleaseVisual,
                actionOnDestroy: OnDestroyVisual,
                collectionCheck: true,
                defaultCapacity: DefaultCapacity,
                maxSize: MaxPoolSize);

            _visualPools[type] = pool;
            return pool;
        }

        CollectibleItemView CreateHost()
        {
            return UnityEngine.Object.Instantiate(_hostPrefab);
        }

        void OnReleaseHost(CollectibleItemView view)
        {
            view.ResetForPool();
            CheckOrCreatePoolRoot();
            view.transform.SetParent(_poolRoot, worldPositionStays: false);
        }

        static void OnDestroyHost(CollectibleItemView view)
        {
            if (view != null) UnityEngine.Object.Destroy(view.gameObject);
        }

        void OnReleaseVisual(GameObject visual)
        {
            if (visual == null) return;
            CheckOrCreatePoolRoot();
            visual.transform.SetParent(_poolRoot, worldPositionStays: false);
        }

        static void OnDestroyVisual(GameObject visual)
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
            _hostPool.Dispose();

            foreach (var pool in _visualPools.Values)
                pool.Dispose();
            _visualPools.Clear();

            if (_poolRoot != null)
            {
                UnityEngine.Object.Destroy(_poolRoot.gameObject);
                _poolRoot = null;
            }
        }
    }
}
