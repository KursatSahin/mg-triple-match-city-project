using System;
using TripleMatch.Core;
using TripleMatch.Data;
using TripleMatch.Level;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    public event Action OnRestartApplication;
    private static RootLifetimeScope _instance;
    public static RootLifetimeScope Instance => _instance;

    [SerializeField] private LevelContainerSO levelContainer;
    [SerializeField] private LevelManagerConfigSO levelManagerConfig;

    #region VContainer Setup

    protected override void Configure(IContainerBuilder builder)
    {
        // Persistent services. Live for the whole app session.
        builder.Register<DataManager>(Lifetime.Singleton).As<IDataManager>();

        builder.Register<LevelManager>(Lifetime.Singleton)
            .WithParameter(levelContainer)
            .WithParameter(levelManagerConfig)
            .As<ILevelManager>();

        // App boot flow: load data, switch from Bootstrap to MainMenu.
        builder.RegisterEntryPoint<GameBootstrapper>();
    }

    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            RestartApplication();
        }
    }

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (_instance != null)
        {
            Debug.LogWarning("Multiple instances of RootLifetimeScope detected. Destroying the new instance.");
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void OnDisable()
    {

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _instance = null;
        Destroy(gameObject);
    }

    public void RestartApplication()
    {
        OnRestartApplication?.Invoke();
        Destroy(gameObject);
    }
    #endregion
}
