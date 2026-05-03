using VContainer;
using VContainer.Unity;

namespace TripleMatch.Core
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<DataManager>(Lifetime.Singleton).As<IDataManager>();
            builder.RegisterEntryPoint<GameBootstrapper>();
        }
    }
}//