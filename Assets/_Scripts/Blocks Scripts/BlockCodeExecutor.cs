#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Xml;
using Newtonsoft.Json;

public class BlockCodeExecutor : MonoBehaviour
{
    public Transform codingArea;
    public static float playStartTime;
    public static bool stopExecution = false;

    private static bool isRunning = false;
    public void OnPlay()
    {
        if (isRunning)
        {
            return;
        }
        ClearUnityConsole();
        Debug.Log("▶ Play started");

        playStartTime = Time.timeSinceLevelLoad;
        stopExecution = false;
        isRunning = true;

        foreach (Transform child in codingArea)
        {
            string layerName = LayerMask.LayerToName(child.gameObject.layer);
            if (layerName.Contains("StartBlock"))
            {
                RunFromStartBlock(child);
            }
        }
    }
 
    public void SaveBlockCode() 
    {
        BlockSaveList saveList = new BlockSaveList();

        foreach (Transform child in codingArea)
        {
            string layerName = LayerMask.LayerToName(child.gameObject.layer);
            if (layerName.Contains("StartBlock"))
            {
                BlockSaveData data = BlockSaveManager.Instance.SaveBlockChain(child);
                if (data != null)
                    saveList.blocks.Add(data);
            }
        }
        string json = JsonConvert.SerializeObject(saveList, Formatting.Indented);

        //string json = JsonUtility.ToJson(saveList, true);

#if UNITY_WEBGL && !UNITY_EDITOR
    WebGLFileDownloader.DownloadJson("block-save.json", json);
#else
        string path = Path.Combine(Application.persistentDataPath, "block-save.json");
        try
        {
            File.WriteAllText(path, json);
            Debug.Log("Saved to: " + path);
        }
        catch (Exception e)
        {
            Debug.LogError("File save failed: " + e.Message);
        }

        Debug.Log(json);
#endif
    }

    private void RunFromStartBlock(Transform startBlock)
    {
        BlockBase block = startBlock.GetComponent<BlockBase>();

        if (block is IConditionalStart conditional)
        {
            Debug.Log($"[BlockGroup] Found conditional start block: {startBlock.name}");
            StartCoroutine(WaitForConditionThenRun(conditional, startBlock));
            return;
        }

        StartCoroutine(ExecuteBlockQueueSequentially(startBlock));
    }

    private IEnumerator WaitForConditionThenRun(IConditionalStart conditional, Transform startBlock)
    {
        Debug.Log($"[BlockGroup] Waiting for condition: {startBlock.name}");

        while (!conditional.IsConditionMet())
        {
            yield return null;
        }

        Debug.Log($"[BlockGroup] Condition met! Executing: {startBlock.name}");
        StartCoroutine(ExecuteBlockQueueSequentially(startBlock));
    }

    private IEnumerator ExecuteBlockQueueSequentially(Transform root)
    {
        Queue<Transform> blockQueue = new Queue<Transform>();
        blockQueue.Enqueue(root);

        while (blockQueue.Count > 0 && !stopExecution)
        {
            Transform current = blockQueue.Dequeue();
            BlockBase currentBlock = current.GetComponent<BlockBase>();

            bool waiting = true;

            if (currentBlock != null)
            {
                currentBlock.Execute(() =>
                {
                    waiting = false;
                });
            }
            else
            {
                waiting = false;
            }

            while (waiting && !stopExecution)
                yield return null;

            if (stopExecution)
            {
                currentBlock.StopAllCoroutines();
                break;
            }

            foreach (Transform child in current)
            {
                if (child.CompareTag("Block"))
                {
                    blockQueue.Enqueue(child);
                }
            }
        }
        
        Debug.Log("✅ BlockGroup: Execution finished or stopped.");
        isRunning = false;
    }



    public static void ClearUnityConsole()
    {
#if UNITY_EDITOR
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod?.Invoke(null, null);
#endif
    }
    public static void StopExecution()
    {
        stopExecution = true;
        MotorSimulationManager.Instance.StopAllMotors();
        //Debug.Log("🛑 BlockGroup: Execution stopped by StopAllBlock.");
    }

}
