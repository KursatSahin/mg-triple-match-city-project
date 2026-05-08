using TripleMatch.Board;
using TripleMatch.Command;
using TripleMatch.Data;
using TripleMatch.Deck;
using TripleMatch.Level;
using TripleMatch.StateMachine;
using TripleMatch.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TripleMatchCity.Runtime.Scopes
{
    public class GameSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameObject levelRootPrefab;
        [SerializeField] private Transform sceneParent;
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private DeckView deckView;
        [SerializeField] private DeckConfigSO deckConfig;
        [SerializeField] private GoalPanelView goalPanelView;
        [SerializeField] private TimerPanelView timerPanelView;
        [SerializeField] private EndGamePopupView endGamePopupView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(cameraController).As<ICameraController>();

            builder.Register<BoardManager>(Lifetime.Singleton).WithParameter(levelRootPrefab).AsImplementedInterfaces();

            builder.RegisterComponent(deckView);
            builder.Register<DeckManager>(Lifetime.Singleton).WithParameter(deckConfig).As<IDeckManager>();

            builder.Register<CommandQueue>(Lifetime.Singleton).As<ICommandQueue>();

            builder.Register<GoalManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterComponent(goalPanelView);
            builder.Register<GoalPanelPresenter>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<TimerManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterComponent(timerPanelView);
            builder.Register<TimerPanelPresenter>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<GameStateMachine>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<UIService>(Lifetime.Singleton).As<IUIService>();

            builder.RegisterComponent(endGamePopupView);
            builder.Register<EndGamePopupPresenter>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterComponent(inputHandler);

            builder.RegisterEntryPoint<GameSceneEntryPoint>().WithParameter<Transform>(sceneParent);
        }
    }
}
