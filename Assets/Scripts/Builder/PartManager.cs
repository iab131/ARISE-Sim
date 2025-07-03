using UnityEngine;

public class PartManager : MonoBehaviour
{
    public static PartManager Instance;

    public Transform spawnRoot;
    public Camera sceneCamera;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnPartInCenter(GameObject prefab)
    {
        Vector3 spawnPos = Vector3.zero; // or some fixed center point
        Instantiate(prefab, spawnPos, Quaternion.identity, spawnRoot);
    }

    public void SpawnPartUnderCursor(GameObject prefab)
    {
        Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Instantiate(prefab, hit.point, Quaternion.identity, spawnRoot);
        }
        else
        {
            // fallback to default position
            Instantiate(prefab, ray.GetPoint(5f), Quaternion.identity, spawnRoot);
        }
    }
}
