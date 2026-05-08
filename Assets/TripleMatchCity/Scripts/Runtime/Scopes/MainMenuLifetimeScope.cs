using TripleMatch.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TripleMatchCity.Runtime.Scopes
{
    public class MainMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] private MainMenuView mainMenuView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(mainMenuView);
            builder.Register<MainMenuPresenter>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
