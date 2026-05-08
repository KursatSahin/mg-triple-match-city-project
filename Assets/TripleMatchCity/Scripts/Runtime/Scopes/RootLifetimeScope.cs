using TripleMatch.Board;
using TripleMatch.Core;
using TripleMatch.Core.Data;
using TripleMatch.Data;
using TripleMatch.Level;
using TripleMatch.Runtime;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TripleMatchCity.Runtime.Scopes
{
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private LevelContainerSO levelContainer;
        [SerializeField] private LevelManagerConfigSO levelManagerConfig;
        [SerializeField] private CollectibleItemView collectibleItemViewPrefab;
        [SerializeField] private ItemPoolConfigSO itemPoolConfig;

        #region VContainer Setup

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(levelContainer).AsSelf();

            builder.RegisterInstance(levelManagerConfig).AsSelf();
            
            builder.RegisterInstance(itemPoolConfig).AsSelf();

            // Persistent services. Live for the whole app session.
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();

            builder.Register<PlayerPrefsProvider>(Lifetime.Singleton).As<IDataProvider>();

            builder.Register<DataManager>(Lifetime.Singleton).As<IDataManager>();

            builder.Register<LevelManager>(Lifetime.Singleton).As<ILevelManager>();

            // Moved ItemFactory here from GameSceneLifeTimeScope
            // ItemFactory lives at the root so its DontDestroyOnLoad pool root persists across
            // Bootstrap -> MainMenu -> Game scene transitions instead of being destroyed when
            // GameSceneLifetimeScope disposes.
            builder.Register<ItemFactory>(Lifetime.Singleton).WithParameter(collectibleItemViewPrefab).As<IItemFactory>();

            // Routes scene transitions for MainMenu / Game requests.
            builder.Register<SceneFlowService>(Lifetime.Singleton).AsImplementedInterfaces();

            // App boot flow: load data, switch from Bootstrap to MainMenu.
            builder.RegisterEntryPoint<GameBootstrapper>();
        }

        #endregion
    }
}
