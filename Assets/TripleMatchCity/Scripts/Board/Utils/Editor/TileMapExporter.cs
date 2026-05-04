using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public class TilemapExporter : EditorWindow
{
    private int    pixelsPerUnit = 32;
    private string outputPath   = "Assets/ExportedMap.png";

    [MenuItem("Tools/Tilemap Exporter")]
    public static void ShowWindow() => GetWindow<TilemapExporter>("Tilemap Exporter");

    void OnGUI()
    {
        GUILayout.Label("Tilemap Export", EditorStyles.boldLabel);
        pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", pixelsPerUnit);
        outputPath    = EditorGUILayout.TextField("Output Path", outputPath);
        EditorGUILayout.HelpBox(
            "Hierarchy'de Tilemap objelerini seçin, sonra butona basın.",
            MessageType.Info);
        if (GUILayout.Button("Export Selected Tilemaps to PNG"))
            ExportTilemaps();
    }

    void ExportTilemaps()
    {
        var selected = Selection.gameObjects;
        if (selected.Length == 0) { Debug.LogError("Hiç obje seçili değil!"); return; }

        Bounds combinedBounds = new Bounds();
        bool   first          = true;

        foreach (var go in selected)
        {
            var tm = go.GetComponent<Tilemap>();
            if (tm == null) continue;

            tm.CompressBounds();

            // CellToWorld ile gerçek world-space köşeleri al
            BoundsInt cells = tm.cellBounds;
            Vector3 worldMin = tm.CellToWorld(cells.min);
            Vector3 worldMax = tm.CellToWorld(cells.max);

            var worldBounds = new Bounds();
            worldBounds.SetMinMax(
                new Vector3(Mathf.Min(worldMin.x, worldMax.x),
                            Mathf.Min(worldMin.y, worldMax.y), 0f),
                new Vector3(Mathf.Max(worldMin.x, worldMax.x),
                            Mathf.Max(worldMin.y, worldMax.y), 0f));

            if (first) { combinedBounds = worldBounds; first = false; }
            else          combinedBounds.Encapsulate(worldBounds);
        }

        if (first) { Debug.LogError("Seçili objeler arasında Tilemap bulunamadı!"); return; }

        int width  = Mathf.RoundToInt(combinedBounds.size.x * pixelsPerUnit);
        int height = Mathf.RoundToInt(combinedBounds.size.y * pixelsPerUnit);
        Debug.Log($"Bounds: {combinedBounds} | Boyut: {width}x{height}px");

        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

        var camGO = new GameObject("_ExportCam");
        var cam   = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = Color.clear;
        cam.cullingMask      = -1;
        cam.targetTexture    = rt;
        cam.aspect           = (float)width / height;
        cam.orthographicSize = combinedBounds.size.y / 2f;
        cam.transform.position = new Vector3(
            combinedBounds.center.x,
            combinedBounds.center.y,
            -100f);

        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        File.WriteAllBytes(outputPath, tex.EncodeToPNG());

        RenderTexture.active = null;
        cam.targetTexture    = null;
        DestroyImmediate(camGO);
        DestroyImmediate(rt);
        DestroyImmediate(tex);

        AssetDatabase.Refresh();
        Debug.Log($"✓ Export tamamlandı → {outputPath} ({width}x{height}px)");
    }
}