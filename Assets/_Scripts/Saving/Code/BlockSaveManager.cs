using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton manager for saving block chains into serializable BlockSaveData.
/// </summary>
public class BlockSaveManager : MonoBehaviour
{
    public static BlockSaveManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // optional: keep across scenes
    }

    /// <summary>
    /// Recursively saves a chain of blocks starting from the given transform.
    /// </summary>
    /// <param name="block">The root block transform.</param>
    /// <returns>A BlockSaveData tree representing the block chain.</returns>
    public BlockSaveData SaveBlockChain(Transform block)
    {
        if (block == null) return null;

        BlockSaveData data = new BlockSaveData
        {
            prefabName = block.name.Replace("(Clone)", "").Trim(),
            position = block.localPosition,
            inputs = new Dictionary<string, string>()
        };

        if (block.TryGetComponent<IBlockSavable>(out var savable))
            data.inputs = savable.SaveInputs();

        // Recursively look for the next "Block"-tagged child (the one chained below)
        foreach (Transform child in block)
        {
            if (child.CompareTag("Block"))
            {
                data.nextBlock = SaveBlockChain(child);
                Debug.Log(child.gameObject.name);
                if (data.nextBlock != null)
                {
                    Debug.Log("→ Saved next block: " + data.nextBlock.prefabName);
                }

            }
        }

        return data;
    }

    public void SaveToJsonFile(Transform startBlock)
    {
        BlockSaveData data = SaveBlockChain(startBlock);
        string json = JsonUtility.ToJson(data, true); // pretty format

#if UNITY_WEBGL && !UNITY_EDITOR
    WebGLFileDownloader.DownloadJson("block-save.json", json);
#else
        string path = System.IO.Path.Combine(Application.persistentDataPath, "block-save.json");
        System.IO.File.WriteAllText(path, json);
        Debug.Log($"Block chain saved to: {path}");
#endif
    }

}
