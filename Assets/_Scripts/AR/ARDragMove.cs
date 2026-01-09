using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// Allows dragging the placed mat along detected planes.
/// </summary>
public class ARDragMove : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    private static readonly List<ARRaycastHit> hits = new();

    private bool isDragging;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Drag(Input.mousePosition);
        }
#else
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
                Drag(t.position);
        }
#endif
    }

    private void Drag(Vector2 screenPos)
    {
        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            return;

        transform.position = hits[0].pose.position;
    }
}
