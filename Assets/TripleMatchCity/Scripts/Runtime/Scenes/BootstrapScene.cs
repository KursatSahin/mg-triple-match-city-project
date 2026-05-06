using TripleMatch.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace TripleMatchCity.Scripts.Runtime.Scenes
{
    public class BootstrapScene : MonoBehaviour
    {
        public const string SceneName = "Bootstrap";

        private void Start()
        {
            //TODO: Setup
            ValidateRootLifetimeScope();
        }

        private void ValidateRootLifetimeScope()
        {
            if (RootLifetimeScope.Instance == null)
            {
                VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance();
            }
        }

        public static void LoadScene()
        {
            
        }
    }
}