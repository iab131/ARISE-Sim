using UnityEngine;

public class BlockPrefabRegistry : MonoBehaviour
{
    public static BlockPrefabRegistry Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject GetPrefab(string name)
    {
        // Load prefab from Resources/Blocks folder
        GameObject prefab = Resources.Load<GameObject>("Blocks/" + name);
        if (prefab == null)
        {
            Debug.LogError($"[BlockPrefabRegistry] Prefab not found in Resources/Blocks/: {name}");
        }
        return prefab;
    }
}
