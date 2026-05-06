using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace TripleMatch.Data.Editor
{
    [CustomEditor(typeof(LevelDataSO))]
    public class LevelDataSOEditor : NaughtyInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var level = (LevelDataSO)target;
            DrawSummary(level);
        }

        static void DrawSummary(LevelDataSO level)
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Item Summary", EditorStyles.boldLabel);

            int collectibleCount = level.CollectibleItems?.Count ?? 0;
            int nonCollectibleCount = level.NonCollectibleItems?.Count ?? 0;

            if (collectibleCount == 0 && nonCollectibleCount == 0)
            {
                EditorGUILayout.LabelField("(No items in this level)", EditorStyles.miniLabel);
                return;
            }

            if (collectibleCount > 0)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField($"Collectible ({collectibleCount})", EditorStyles.miniBoldLabel);
                DrawTypeCounts(level.CollectibleItems);
            }

            if (nonCollectibleCount > 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField($"Non-Collectible ({nonCollectibleCount})", EditorStyles.miniBoldLabel);
                DrawTypeCounts(level.NonCollectibleItems);
            }
        }

        static void DrawTypeCounts(List<CollectibleItemInstanceData> items)
        {
            var counts = new Dictionary<CollectibleItemData, int>();
            foreach (var item in items)
            {
                if (item == null || item.Item == null) continue;
                counts.TryGetValue(item.Item, out var c);
                counts[item.Item] = c + 1;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                foreach (var kvp in counts.OrderByDescending(k => k.Value).ThenBy(k => k.Key.name))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.ObjectField(kvp.Key, typeof(CollectibleItemData), false);
                        EditorGUILayout.LabelField($"× {kvp.Value}", GUILayout.Width(50));
                    }
                }
            }
        }
    }
}
