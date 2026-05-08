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

        #region VContainer Setup

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(levelContainer).AsSelf();
        
            builder.RegisterInstance(levelManagerConfig).AsSelf();
        
            // Persistent services. Live for the whole app session.
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();

            builder.Register<PlayerPrefsProvider>(Lifetime.Singleton).As<IDataProvider>();
        
            builder.Register<DataManager>(Lifetime.Singleton).As<IDataManager>();
        
            builder.Register<LevelManager>(Lifetime.Singleton).As<ILevelManager>();

            // Routes scene transitions for MainMenu / Game requests.
            builder.Register<SceneFlowService>(Lifetime.Singleton).AsImplementedInterfaces();

            // App boot flow: load data, switch from Bootstrap to MainMenu.
            builder.RegisterEntryPoint<GameBootstrapper>();
        }

        #endregion
    }
}
