using TripleMatch.Board;
using TripleMatch.Level;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class GameSceneLifetimeScope : LifetimeScope
{
    [SerializeField] private GameObject levelRootPrefab;
    [SerializeField] private CollectibleItemView collectibleItemViewPrefab;
    [SerializeField] private Transform sceneParent;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ItemFactory>(Lifetime.Singleton).WithParameter(collectibleItemViewPrefab).As<IItemFactory>();

        builder.Register<BoardManager>(Lifetime.Singleton).WithParameter(levelRootPrefab).As<IBoardManager>();

        builder.RegisterEntryPoint<GameSceneEntryPoint>().WithParameter<Transform>(sceneParent);
    }
}
