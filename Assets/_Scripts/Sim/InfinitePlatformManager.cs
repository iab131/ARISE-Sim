using UnityEngine;

public class InfinitePlatformManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform robot;
    public int gridSize = 3;
    public float tileSize = 20f;

    private GameObject[,] tiles;

    void Start()
    {
        tiles = new GameObject[gridSize, gridSize];
        Vector3 center = robot.position;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 pos = new Vector3(
                    (x - gridSize / 2) * tileSize,
                    0,
                    (z - gridSize / 2) * tileSize
                );
                tiles[x, z] = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
            }
        }
    }

    void Update()
    {
        Vector3 center = robot.position;
        int centerX = Mathf.RoundToInt(center.x / tileSize);
        int centerZ = Mathf.RoundToInt(center.z / tileSize);

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                int offsetX = x - gridSize / 2;
                int offsetZ = z - gridSize / 2;

                Vector3 newPos = new Vector3(
                    (centerX + offsetX) * tileSize,
                    0,
                    (centerZ + offsetZ) * tileSize
                );
                tiles[x, z].transform.position = newPos;
            }
        }
    }
}
