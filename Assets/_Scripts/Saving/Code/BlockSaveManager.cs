#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SFB; // Add at the top
/// <summary>
/// Singleton manager for saving block chains into serializable BlockSaveData.
/// </summary>
public class BlockSaveManager : MonoBehaviour
{
    public static BlockSaveManager Instance { get; private set; }
    public Transform codingArea;
    private void Awake()
    {
        // Singleton pattern setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }

        Instance = this;
    }
#if UNITY_WEBGL && !UNITY_EDITOR
[DllImport("__Internal")]
    private static extern void UploadJsonFile(string gameObjectName, string methodName);

[DllImport("__Internal")]
private static extern void DownloadFile(string filename, string content);
#endif

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
            position = new SerializableVector2(block.localPosition),
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
                //Debug.Log(child.gameObject.name);
            }
        }

        return data;
    }

    public void SaveBlockCode()
    {
        BlockSaveList saveList = new BlockSaveList();

        foreach (Transform child in codingArea)
        {
            string layerName = LayerMask.LayerToName(child.gameObject.layer);
            if (layerName.Contains("StartBlock"))
            {
                BlockSaveData data = SaveBlockChain(child);
                if (data != null)
                    saveList.blocks.Add(data);
            }
        }
        string json = JsonConvert.SerializeObject(saveList, Formatting.Indented);
        string encryptedJson = JsonEncryptor.Encrypt(json);

        //string json = JsonUtility.ToJson(saveList, true);

#if UNITY_WEBGL && !UNITY_EDITOR
    DownloadFile("mycode.fllcode", encryptedJson);
#else
        SaveAs(encryptedJson);

        //string path = Path.Combine(Application.persistentDataPath, "block-save.json");
        //try
        //{
        //    string encryptedJson = JsonEncryptor.Encrypt(json);
        //    File.WriteAllText(path, encryptedJson);
        //    Debug.Log("Saved to: " + path);
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError("File save failed: " + e.Message);
        //}
#endif
    }

    private void SaveAs(string encrypted)
    {
        string path = StandaloneFileBrowser.SaveFilePanel("Save Block Code", "", "BlockCode", "fllcode");
        if (!string.IsNullOrEmpty(path))
        {
            if (!path.EndsWith(".fllcode"))
                path += ".fllcode";
            File.WriteAllText(path, encrypted);
            Debug.Log("Saved to: " + path);
        }
    }
    public void LoadBlockCode(string json)
    {
        foreach (Transform child in codingArea)
        {
            Destroy(child.gameObject);
        }

        BlockSaveList saveList = JsonConvert.DeserializeObject<BlockSaveList>(json);

        foreach (BlockSaveData chainRoot in saveList.blocks)
        {
            SpawnBlockChain(chainRoot, codingArea); // codingArea is your UI container
        }
    }

    public void SpawnBlockChain(BlockSaveData data, Transform parent)
    {
        if (data == null) return;

        GameObject prefab = BlockPrefabRegistry.Instance.GetPrefab(data.prefabName);
        if (prefab == null)
        {
            Debug.LogError("Prefab not found for: " + data.prefabName);
            return;
        }

        GameObject blockGO = Instantiate(prefab, parent);
        blockGO.tag = "Block";
        RectTransform rect = blockGO.GetComponent<RectTransform>();
        rect.localPosition = data.position.ToVector2();

        // Load block input fields
        if (blockGO.TryGetComponent<IBlockSavable>(out var savable))
            savable.LoadInputs(data.inputs);

        // Chain next block under this block
        if (data.nextBlock != null)
            SpawnBlockChain(data.nextBlock, blockGO.transform);
    }
    public void OnClickLoadJsonWebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    UploadJsonFile(gameObject.name, "OnRobotJsonLoaded");
#else
        LoadFromFilePicker();
        //string path = Path.Combine(Application.persistentDataPath, "block-save.json");

        //if (!File.Exists(path))
        //{
        //    Debug.LogWarning("No save file found at: " + path);
        //    return;
        //}

        //try
        //{
        //    string encryptedJson = File.ReadAllText(path);
        //    string json = JsonEncryptor.Decrypt(encryptedJson);

        //    //Debug.Log("Loaded JSON from disk:\n" + json);
        //    LoadBlockCode(json);
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError("Failed to load save file: " + e.Message);
        //}
#endif
    }
    public void OnJsonFileLoaded(string encryptedJson)
    {
        string json = JsonEncryptor.Decrypt(encryptedJson);
        LoadBlockCode(json);
    }

    private void LoadFromFilePicker()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open Block Code", "", "fllcode", false);
        if (paths.Length > 0 && File.Exists(paths[0]))
        {
            string path = paths[0];

            if (Path.GetExtension(path) != ".fllcode")
            {
                Debug.LogWarning("Wrong file type selected.");
                return;
            }
            string encrypted = File.ReadAllText(path);
            string json = JsonEncryptor.Decrypt(encrypted);

            // Optional: Clear existing blocks
            foreach (Transform child in codingArea)
            {
                Destroy(child.gameObject);
            }

            LoadBlockCode(json);
        }
    }

}
