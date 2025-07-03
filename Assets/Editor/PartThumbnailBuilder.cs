#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary> 
/// Builds square PNG thumbnails for every prefab under Parts/ and drops
/// them into Assets/Generated/Thumbnails as read-only sprites.
/// </summary>
public static class PartThumbnailBuilder
{
    private const string PREFAB_FOLDER = "Assets/3D Parts";       // ← your prefabs
    private const string THUMB_FOLDER = "Assets/Images/Thumbnails";
    private const int SIZE = 256;                  // px

    [MenuItem("Tools/ARISE/Generate Part Thumbnails")]
    public static void GenerateAll()
    {
        // Ensure destination exists
        Directory.CreateDirectory(THUMB_FOLDER);

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab",
                                new[] { PREFAB_FOLDER });

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);

            // Ask Unity for its cached preview – returns null until ready
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            if (preview == null) { continue; }

            // Encode → PNG
            byte[] png = preview.EncodeToPNG();

            // Build filename: Parts/Wheel_32.prefab → Wheel_32.png
            string fileName = $"{Path.GetFileNameWithoutExtension(prefabPath)}.png";
            string filePath = Path.Combine(THUMB_FOLDER, fileName);

            File.WriteAllBytes(filePath, png);
        }

        AssetDatabase.Refresh(); // Import new images as Sprites
        Debug.Log("Part thumbnails generated ✅");
    }
}
#endif