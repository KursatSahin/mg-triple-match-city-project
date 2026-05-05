using System.IO;
using UnityEditor;
using UnityEngine;

namespace TripleMatch.Data.Editor
{
    public static class CollectibleItemDataGenerator
    {
        const string OutputFolder = "Assets/TripleMatchCity/Data/Items";
        const string MenuPath = "Assets/TripleMatch/Generate Collectible Item Data";

        [MenuItem(MenuPath, priority = 50)]
        static void GenerateFromSelection()
        {
            var selected = Selection.objects;
            if (selected == null || selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Generate Collectible Item Data",
                    "No prefabs selected in the Project window.", "OK");
                return;
            }

            EnsureFolder(OutputFolder);

            int created = 0;
            int skipped = 0;

            foreach (var obj in selected)
            {
                if (obj is not GameObject go || !PrefabUtility.IsPartOfPrefabAsset(go))
                {
                    Debug.LogWarning(
                        $"[Generate Collectible Item Data] Skipping '{obj.name}': not a prefab asset.");
                    skipped++;
                    continue;
                }

                var displayName = $"{go.name}_Data";
                var assetPath = $"{OutputFolder}/{displayName}.asset";

                if (AssetDatabase.LoadAssetAtPath<CollectibleItemData>(assetPath) != null)
                {
                    Debug.LogWarning(
                        $"[Generate Collectible Item Data] '{displayName}.asset' already exists, skipping.");
                    skipped++;
                    continue;
                }

                var data = ScriptableObject.CreateInstance<CollectibleItemData>();
                data.DisplayName = displayName;
                data.VisualPrefab = go;

                AssetDatabase.CreateAsset(data, assetPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Generate Collectible Item Data] Created: {created}, Skipped: {skipped}.");
        }

        [MenuItem(MenuPath, validate = true)]
        static bool ValidateGenerateFromSelection()
        {
            if (Selection.objects == null) return false;
            foreach (var obj in Selection.objects)
            {
                if (obj is GameObject go && PrefabUtility.IsPartOfPrefabAsset(go))
                    return true;
            }
            return false;
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
