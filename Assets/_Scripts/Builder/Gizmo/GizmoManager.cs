using System.Collections.Generic;
using UnityEngine;

public class GizmoManager : MonoBehaviour
{
    public static GizmoManager Instance { get; private set; }

    [Header("Handle Prefabs")]
    [SerializeField] private GameObject rotateLeftHandlePrefab;
    [SerializeField] private GameObject rotateRightHandlePrefab;
    [SerializeField] private GameObject moveUpHandlePrefab;
    [SerializeField] private GameObject moveDownHandlePrefab;

    private GameObject currentHole;
    private readonly List<GameObject> activeHandles = new();
    private bool firstIsHole;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowHandles(GameObject hole, bool first, bool isAxle)
    {
        ClearHandles();
        currentHole = hole;
        firstIsHole = first;
        Vector3 pos = hole.transform.position;
        Vector3 axis = hole.transform.up;
        Vector3 up = hole.transform.up;         // Axis for move
        Vector3 right = hole.transform.right;   // Local right for rotation spacing
        Vector3 forward = hole.transform.forward; // Optional if you want depth-based positioning
        float offset = 0.5f;
        float offset2 = 1;

        if (isAxle){
            // Move Up Handle: along up axis, should look outward from hole (aligned with up)
        activeHandles.Add(Instantiate(
            moveUpHandlePrefab,
            pos + forward * offset2 + up * offset2,
            Quaternion.LookRotation(right,up),
            hole.transform // Parent to hole
        ));
        
        // Move Down Handle: opposite direction
        activeHandles.Add(Instantiate(
            moveDownHandlePrefab,
            pos + forward * offset2 - up * offset2,
            Quaternion.LookRotation(right,up),
            hole.transform // Parent to hole
        ));

        }


        // Rotate Left Handle: place to the left, facing around the axis (like a curved ring)
        activeHandles.Add(Instantiate(
            rotateLeftHandlePrefab,
            pos - right * offset,
            Quaternion.LookRotation(right, up),
            hole.transform // Parent to hole
        ));

        // Rotate Right Handle: place to the right, facing opposite around the ring
        activeHandles.Add(Instantiate(
            rotateRightHandlePrefab,
            pos + right * offset,
            Quaternion.LookRotation(right, up),
            hole.transform // Parent to hole
        ));

        

    }

    public void ClearHandles()
    {
        foreach (GameObject h in activeHandles)
            Destroy(h);
        activeHandles.Clear();
    }

    public void OnHandleClicked(GizmoHandler.ActionType action)
    { 
        int m = 1;
        if (firstIsHole){
            m = -1;
        }
        switch (action)
        {
            case GizmoHandler.ActionType.RotateLeft:
                ControlManager.Instance.RotateHoleAroundAxis("up", -45f * m);
                break;
            case GizmoHandler.ActionType.RotateRight:
                ControlManager.Instance.RotateHoleAroundAxis("up", 45f * m);
                break;
            case GizmoHandler.ActionType.MoveUp:
                ControlManager.Instance.MoveHoleAlongAxis(1 * m);
                break;
            case GizmoHandler.ActionType.MoveDown:
                ControlManager.Instance.MoveHoleAlongAxis(-1 * m);
                break;
        }

        // You can call your SnapPartToHole logic here if needed
    }
}
