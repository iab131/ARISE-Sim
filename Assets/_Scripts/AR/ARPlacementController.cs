using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Handles interactive AR placement of the mat,
/// then spawns the robot aligned to the mat.
/// </summary>
public class ARPlacementController : MonoBehaviour
{
    [Header("References")]
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    public GameObject matPreviewPrefab;
    public GameObject simulationRootPrefab;

    [Header("Placement State")]
    private GameObject matPreview;
    private GameObject simulationRoot;
    private Pose currentPose;
    private bool poseIsValid;

    private static readonly List<ARRaycastHit> hits = new();

    void Start()
    {
        matPreview = Instantiate(matPreviewPrefab);
        matPreview.SetActive(false);
    }

    void Update()
    {
        UpdatePose();
        UpdatePreview();

        HandleConfirmPlacement();
    }

    private void UpdatePose()
    {
#if UNITY_EDITOR
        Vector2 pos = Input.mousePosition;
#else
        if (Input.touchCount == 0)
        {
            poseIsValid = false;
            return;
        }
        Vector2 pos = Input.GetTouch(0).position;
#endif

        poseIsValid = raycastManager.Raycast(pos, hits, TrackableType.PlaneWithinPolygon);
        if (poseIsValid)
        {
            currentPose = hits[0].pose;
        }
    }

    private void UpdatePreview()
    {
        if (!poseIsValid) return;

        if (!matPreview.activeSelf)
            matPreview.SetActive(true);

        matPreview.transform.SetPositionAndRotation(
            currentPose.position,
            Quaternion.Euler(0, currentPose.rotation.eulerAngles.y, 0)
        );
    }

    private void HandleConfirmPlacement()
    {
#if UNITY_EDITOR
        if (!Input.GetMouseButtonDown(0)) return;
#else
        if (Input.touchCount == 0) return;
        if (Input.GetTouch(0).phase != TouchPhase.Began) return;
#endif

        if (!poseIsValid || simulationRoot != null) return;

        simulationRoot = Instantiate(
            simulationRootPrefab,
            matPreview.transform.position,
            matPreview.transform.rotation
        );

        AlignRobotToMat(simulationRoot);
        FinalizePlacement();
    }

    private void AlignRobotToMat(GameObject root)
    {
        Transform mat = root.transform.Find("Mat");
        Transform robot = root.transform.Find("Robot");

        robot.position = mat.position + mat.forward * 0.3f; // start zone
        robot.rotation = mat.rotation;
    }

    private void FinalizePlacement()
    {
        matPreview.SetActive(false);

        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(false);

        planeManager.enabled = false;
    }

    /// <summary>
    /// Called by UI button to confirm placement.
    /// Locks the mat & enables simulation.
    /// </summary>
    public void ConfirmPlacement()
    {
        if (simulationRoot == null)
            return;

        // Disable plane visuals
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(false);

        planeManager.enabled = false;

        // Optionally disable placement scripts
        var drag = simulationRoot.GetComponent<ARDragMove>();
        if (drag) drag.enabled = false;

        var rotate = simulationRoot.GetComponent<ARRotate>();
        if (rotate) rotate.enabled = false;

        Debug.Log("AR Placement Confirmed");
    }

    public void ResetSimulation()
    {
        if (simulationRoot != null)
            Destroy(simulationRoot);

        planeManager.enabled = true;

        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(true);
    }

}
