using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TripleMatch.Core
{
    public static class EventBusUtil
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticFields()
        {
            EventBusRegistry.ClearAll();
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                EventBusRegistry.ClearAll();
            }
        }
#endif
    }
}