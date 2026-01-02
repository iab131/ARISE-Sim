/*
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
*/


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class PartThumbnailBuilder
{
    private const string PREFAB_FOLDER = "Assets/3D Parts";
    private const string THUMB_FOLDER = "Assets/Images/Thumbnails";
    private const int SIZE = 256;

    [MenuItem("Tools/ARISE/Generate Part Thumbnails (Custom BG)")]
    public static void GenerateAll()
    {
        Directory.CreateDirectory(THUMB_FOLDER);

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PREFAB_FOLDER });

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) continue;

            Texture2D thumb = RenderThumbnail(prefab);

            string fileName = Path.GetFileNameWithoutExtension(path) + ".png";
            File.WriteAllBytes(Path.Combine(THUMB_FOLDER, fileName), thumb.EncodeToPNG());
        }

        AssetDatabase.Refresh();
        Debug.Log("Custom thumbnails generated ✅");
    }

    private static Texture2D RenderThumbnail(GameObject prefab)
    {
        PreviewRenderUtility preview = new PreviewRenderUtility();
        preview.camera.nearClipPlane = 0.01f;
        preview.camera.farClipPlane = 1000f;
        preview.cameraFieldOfView = 30f;

        // 👇 LIGHT GREY BACKGROUND
        preview.camera.backgroundColor = new Color(0.86f, 0.88f, 0.90f);
        preview.camera.clearFlags = CameraClearFlags.SolidColor;

        GameObject instance = preview.InstantiatePrefabInScene(prefab);

        // Rotate specific parts up by 90°
        string name = prefab.name.ToLower();

        if (name.Contains("Angle Element"))
        {
            instance.transform.rotation = Quaternion.Euler(180f, -90f,0f);
        }
        else if (name == "bionicle eye")
        {
            instance.transform.rotation = Quaternion.Euler(180f, 180f, 0f);
        }
        else
        {
            instance.transform.rotation = Quaternion.identity;
        }




        Bounds bounds = GetBounds(instance);

        // Move model so its visual center is at origin
        instance.transform.position = -bounds.center;

        // Recalculate bounds after move
        bounds = GetBounds(instance);


        // Fixed catalog angle
        Quaternion rot = Quaternion.Euler(25f, -35f, 0f);
        preview.camera.transform.rotation = rot;

        // Get the largest size of the object
        float radius = bounds.extents.magnitude;

        // Camera FOV math (this is the key)
        float fov = preview.camera.fieldOfView * Mathf.Deg2Rad;
        float distance = radius / Mathf.Sin(fov * 0.5f);

        // Add padding so it never touches edges
        distance *= 1.15f;

        // Position camera
        Vector3 camPos = bounds.center + rot * Vector3.back * distance;
        preview.camera.transform.position = camPos;
        Vector3 target = Vector3.zero;
        preview.camera.transform.LookAt(target);
        preview.camera.transform.LookAt(target);



        preview.lights[0].intensity = 1.4f;
        preview.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
        preview.lights[1].intensity = 1.2f;

        preview.BeginPreview(new Rect(0, 0, SIZE, SIZE), GUIStyle.none);
        preview.camera.Render();

        Texture rendered = preview.EndPreview();

        // Copy Texture → Texture2D
        RenderTexture rt = RenderTexture.GetTemporary(SIZE, SIZE, 24);
        Graphics.Blit(rendered, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, SIZE, SIZE), 0, 0);
        tex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        preview.Cleanup();
        return tex;

    }

    private static Bounds GetBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);
        return bounds;
    }
}
#endif
