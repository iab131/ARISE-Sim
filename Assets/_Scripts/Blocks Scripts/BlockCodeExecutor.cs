#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCodeExecutor : MonoBehaviour
{
    public Transform codingArea;
    public static float playStartTime;
    public static bool stopExecution = false;

    
    public void OnPlay()
    {
        ClearUnityConsole();
        Debug.Log("▶ Play started");

        playStartTime = Time.timeSinceLevelLoad;
        stopExecution = false;

        foreach (Transform child in codingArea)
        {
            string layerName = LayerMask.LayerToName(child.gameObject.layer);
            if (layerName.Contains("StartBlock"))
            {
                RunFromStartBlock(child);
            }
        }
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
                break;

            foreach (Transform child in current)
            {
                if (child.CompareTag("Block"))
                {
                    blockQueue.Enqueue(child);
                }
            }
        }

        Debug.Log("✅ BlockGroup: Execution finished or stopped.");
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
        //Debug.Log("🛑 BlockGroup: Execution stopped by StopAllBlock.");
    }

}
