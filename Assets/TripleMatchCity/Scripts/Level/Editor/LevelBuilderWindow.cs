using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TripleMatch.Data;
using UnityEditor;
using UnityEngine;

namespace TripleMatch.Level.Editor
{
    public class LevelBuilderWindow : EditorWindow
    {
        const string LevelsFolder = "Assets/TripleMatchCity/Data/Levels";
        const string BackgroundChildName = "Background";
        const string CollectibleGroupName = "CollectibleItems";
        const string NonCollectibleGroupName = "NonCollectibleItems";

        Transform _levelRoot;
        LevelDataSO _targetLevelData;

        [MenuItem("TripleMatch/Level/Level Builder")]
        static void Open()
        {
            GetWindow<LevelBuilderWindow>("Level Builder");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Expected structure:\n" +
                "  LevelRoot\n" +
                "  |_ Background (SpriteRenderer)\n" +
                "  |_ CollectibleItems/* (visual prefab instances)\n" +
                "  |_ NonCollectibleItems/* (visual prefab instances)\n\n" +
                "Rule: an item type cannot appear in both groups in the same level.\n",
                MessageType.Info);

            EditorGUILayout.Space();

            _levelRoot = (Transform)EditorGUILayout.ObjectField("Level Root", _levelRoot, typeof(Transform), true);

            _targetLevelData = (LevelDataSO)EditorGUILayout.ObjectField("Target Level Data (optional)", _targetLevelData, typeof(LevelDataSO), false);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_levelRoot == null))
            {
                if (GUILayout.Button("Build / Update", GUILayout.Height(28)))
                {
                    BuildLevelData();
                }
            }
        }

        void BuildLevelData()
        {
            var lookup = BuildVisualToItemDataLookup();
            
            if (lookup.Count == 0)
            {
                Debug.LogError("[Level Builder] No CollectibleItemData assets found in the project.");
                return;
            }

            var collectibleGroup = _levelRoot.Find(CollectibleGroupName);
            var nonCollectibleGroup = _levelRoot.Find(NonCollectibleGroupName);

            if (collectibleGroup == null && nonCollectibleGroup == null)
            {
                Debug.LogError(
                    $"[Level Builder] Neither '{CollectibleGroupName}' nor '{NonCollectibleGroupName}' " +
                    $"child found under '{_levelRoot.name}'.");
                return;
            }

            var background = ExtractBackground(_levelRoot.Find(BackgroundChildName));

            var collectibleItems = new List<CollectibleItemInstanceData>();
            var nonCollectibleItems = new List<CollectibleItemInstanceData>();
            var indexByTransform = new Dictionary<Transform, int>();
            int skipped = 0;

            if (collectibleGroup != null)
                WalkChildren(collectibleGroup, isCollectible: true, lookup, collectibleItems, indexByTransform, ref skipped);

            if (nonCollectibleGroup != null)
                WalkChildren(nonCollectibleGroup, isCollectible: false, lookup, nonCollectibleItems, indexByTransform, ref skipped);

            if (!ValidateNoCrossGroupTypes(collectibleItems, nonCollectibleItems, out var error))
            {
                Debug.LogError($"[Level Builder] Validation failed:\n{error}");
                EditorUtility.DisplayDialog("Level Builder — Validation Failed", error, "OK");
                return;
            }

            var target = _targetLevelData != null ? _targetLevelData : CreateNewLevelDataAsset();

            Undo.RecordObject(target, "Build Level Data");
            target.Background = background;
            target.CollectibleItems = collectibleItems;
            target.NonCollectibleItems = nonCollectibleItems;

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (_targetLevelData == null) _targetLevelData = target;

            Debug.Log(
                $"[Level Builder] Built '{target.name}': " +
                $"{collectibleItems.Count} collectible, {nonCollectibleItems.Count} non-collectible, " +
                $"{skipped} skipped. " +
                $"Background sprite: {(background.Sprite != null ? background.Sprite.name : "<none>")}.");

            EditorGUIUtility.PingObject(target);
        }

        static bool ValidateNoCrossGroupTypes(
            List<CollectibleItemInstanceData> collectibleItems,
            List<CollectibleItemInstanceData> nonCollectibleItems,
            out string error)
        {
            var collectibleTypes = new HashSet<CollectibleItemData>(
                collectibleItems.Where(i => i.Item != null).Select(i => i.Item));
            var nonCollectibleTypes = new HashSet<CollectibleItemData>(
                nonCollectibleItems.Where(i => i.Item != null).Select(i => i.Item));

            var conflicts = collectibleTypes.Intersect(nonCollectibleTypes).ToList();
            if (conflicts.Count == 0)
            {
                error = null;
                return true;
            }

            var sb = new StringBuilder();
            sb.AppendLine("The following item type(s) appear in both groups:");
            foreach (var type in conflicts)
                sb.AppendLine($"  • {type.name}");
            sb.AppendLine();
            sb.AppendLine("Move all instances of each type into a single group, then rebuild.");
            error = sb.ToString();
            return false;
        }

        static Dictionary<GameObject, CollectibleItemData> BuildVisualToItemDataLookup()
        {
            var dict = new Dictionary<GameObject, CollectibleItemData>();
            var guids = AssetDatabase.FindAssets("t:CollectibleItemData");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<CollectibleItemData>(path);
                if (data == null || data.VisualPrefab == null) continue;

                if (dict.TryGetValue(data.VisualPrefab, out var existing))
                {
                    Debug.LogWarning(
                        $"[Level Builder] Visual prefab '{data.VisualPrefab.name}' is referenced by " +
                        $"both '{existing.name}' and '{data.name}'. Using '{existing.name}'.");
                    continue;
                }

                dict[data.VisualPrefab] = data;
            }
            return dict;
        }

        static BackgroundData ExtractBackground(Transform background)
        {
            if (background == null) return new BackgroundData();

            var sr = background.GetComponent<SpriteRenderer>();
            return new BackgroundData
            {
                Sprite = sr != null ? sr.sprite : null,
                Position = background.localPosition,
                Size = sr != null ? sr.size : Vector2.one
            };
        }

        static void WalkChildren(
            Transform parent,
            bool isCollectible,
            Dictionary<GameObject, CollectibleItemData> lookup,
            List<CollectibleItemInstanceData> items,
            Dictionary<Transform, int> indexByTransform,
            ref int skipped)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                var sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (sourcePrefab == null)
                {
                    Debug.LogWarning(
                        $"[Level Builder] '{child.name}' under '{parent.name}' is not a prefab instance, skipping.");
                    skipped++;
                    continue;
                }

                if (!lookup.TryGetValue(sourcePrefab, out var itemData))
                {
                    Debug.LogWarning(
                        $"[Level Builder] No CollectibleItemData references prefab " +
                        $"'{sourcePrefab.name}' (under '{child.name}'), skipping.");
                    skipped++;
                    continue;
                }

                int parentIndex = indexByTransform.TryGetValue(child.parent, out var pi) ? pi : -1;
                var sr = child.GetComponentInChildren<SpriteRenderer>();
                var localScale = child.localScale;

                var instance = new CollectibleItemInstanceData
                {
                    Item = itemData,
                    Position = child.localPosition,
                    Scale = new Vector2(localScale.x, localScale.y),
                    IsMirrored = localScale.x < 0f,
                    SortingOrder = sr != null ? sr.sortingOrder : 0,
                    CollectibleParentIndex = parentIndex,
                    IsCollectible = isCollectible
                };

                int myIndex = items.Count;
                items.Add(instance);
                indexByTransform[child] = myIndex;

                WalkChildren(child, isCollectible, lookup, items, indexByTransform, ref skipped);
            }
        }

        static LevelDataSO CreateNewLevelDataAsset()
        {
            EnsureFolder(LevelsFolder);

            int n = 1;
            string path;
            do
            {
                path = $"{LevelsFolder}/Level_{n:D2}.asset";
                n++;
            }
            while (AssetDatabase.LoadAssetAtPath<LevelDataSO>(path) != null);

            var so = CreateInstance<LevelDataSO>();
            AssetDatabase.CreateAsset(so, path);
            return so;
        }

        static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            var parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
            var leaf = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
