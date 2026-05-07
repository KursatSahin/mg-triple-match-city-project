using TripleMatch.Board;
using TripleMatch.Deck;
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
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private DeckView deckView;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ItemFactory>(Lifetime.Singleton).WithParameter(collectibleItemViewPrefab).As<IItemFactory>();

        builder.Register<BoardManager>(Lifetime.Singleton).WithParameter(levelRootPrefab).As<IBoardManager>();

        builder.RegisterComponent(deckView);
        builder.Register<DeckManager>(Lifetime.Singleton).As<IDeckManager>();

        builder.RegisterComponent(inputHandler);

        builder.RegisterEntryPoint<GameSceneEntryPoint>().WithParameter<Transform>(sceneParent);
    }
}
