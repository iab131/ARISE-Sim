using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ControlManager : MonoBehaviour
{
    /* =============================================================
     *  HIGH-LEVEL MODES
     * =============================================================*/
    private enum Mode { Move, Holes }
    private Mode currentMode = Mode.Move;

    /* =============================================================
     *  SUB-STATE for "Holes" mode (finite-state machine)
     * =============================================================*/
    private enum HoleState { SelectingFirst, SelectingSecond, PreviewAdjust }
    private HoleState holeState = HoleState.SelectingFirst;

    /* =============================================================
     *  AXIS LOCK for part-dragging
     * =============================================================*/
    private enum Axis { None, X, Y, Z }
    private Axis lockedAxis = Axis.None;

    /* =============================================================
     *  CONSTANTS / LAYERS
     * =============================================================*/
    private const int holeLayer = 8;
    private const int partsLayer = 9;
    private const float snapTime = 0.25f;   // seconds for snap animation

    /* =============================================================
     *  HIGHLIGHT RENDERERS & SELECTIONS
     * =============================================================*/
    private Renderer hoverHoleRenderer;
    private Renderer firstHoleRenderer;

    private GameObject firstHoleGO;   // ≙ moving part’s hole
    private GameObject secondHoleGO;  // ≙ target hole
    private Transform hole;

    /* =============================================================
     *  PREVIEW BOOK-KEEPING
     * =============================================================*/
    private Transform movingPartRoot;
    private Vector3 previewOrigPos;
    private Quaternion previewOrigRot;
    private Quaternion previewHoleRot;


    /* =============================================================
     *  DRAGGING
     * =============================================================*/
    private float dragScreenZ;
    private Vector3 dragOffset;
    private GameObject draggedPart;
    private Plane dragPlane;       // plane we drag on
    private Vector3 grabLocal;

    /* =============================================================
     *  PART SPAWNING
     * =============================================================*/
    public static ControlManager Instance { get; private set; }
    public Transform spawnRoot;
    [SerializeField] private float spawnDistance;
    [SerializeField] private ScrollRect blockPaletteScrollRect;
    private Boolean isSpawn = false;

    [Header("Mode UI")]
    [SerializeField] private Button moveModeButton;
    [SerializeField] private Button holesModeButton;


    /* ─────────────────────────────────────────────────────────── */
    private void Awake()
    {
        Instance = this;

        if (moveModeButton != null)
            moveModeButton.onClick.AddListener(MoveMode);

        if (holesModeButton != null)
            holesModeButton.onClick.AddListener(HolesMode);

        UpdateModeButtons();
    }
    void Update()
    {
        if (IsInputFieldFocused() || MotorLabelManager.Instance.IsModalOpen) return;
        HandleModeHotkeys();
        HandleDeletion();

        if (currentMode == Mode.Move)
        {
            HandleAxisLockHotkeys();
            HandlePartDragging();
        }
        else
        {
            HandleHoleHover();
            HandleHoleFSM();
        }
    }

    /* =============================================================
  *  Buttons
  * =============================================================*/
    public void MoveMode()
    {
        currentMode = Mode.Move;
        lockedAxis = Axis.None;
        ClearAllHoleState();
        UpdateModeButtons();
        MotorLabelManager.Instance.SetAssignMode(false);
    }

    public void HolesMode()
    {
        currentMode = Mode.Holes;
        holeState = HoleState.SelectingFirst;
        ClearAllHoleState();
        UpdateModeButtons();
        MotorLabelManager.Instance.SetAssignMode(false);
    }
    private void UpdateModeButtons()
    {
        if (moveModeButton == null || holesModeButton == null) return;

        // quick & clear: active button -> non-interactable + highlighted tint
        bool inMove = currentMode == Mode.Move;

        moveModeButton.interactable = !inMove;
        holesModeButton.interactable = inMove;
    }

    bool IsInputFieldFocused()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return false;
        return selected.GetComponent<TMP_InputField>() != null;
    }

    /* =============================================================
     *  MODE SWITCHES
     * =============================================================*/
    void HandleModeHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MoveMode();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            HolesMode();
        }
    }

    /* =============================================================
     *  AXIS-LOCK HOTKEYS
     * =============================================================*/
    void HandleAxisLockHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.X)) lockedAxis = (lockedAxis == Axis.X) ? Axis.None : Axis.X;
        else if (Input.GetKeyDown(KeyCode.Y)) lockedAxis = (lockedAxis == Axis.Y) ? Axis.None : Axis.Y;
        else if (Input.GetKeyDown(KeyCode.Z)) lockedAxis = (lockedAxis == Axis.Z) ? Axis.None : Axis.Z;
        else if (Input.GetKeyDown(KeyCode.Escape)) lockedAxis = Axis.None;
    }

    /* =============================================================
     *  DELETING PARTS
     * =============================================================*/
    void HandleDeletion()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
        {
            GameObject hit = GetObjectUnderCursor(partsLayer);
            if (GetGroupRoot(hit.transform).childCount > 1)
            {
                Destroy(hit);
            }
            else
            {
                Destroy(GetGroupRoot(hit.transform).gameObject);
            }
        }
    }

    /* =============================================================
     *  PART DRAGGING (unchanged)
     * =============================================================*/
    void HandlePartDragging()
    {
        int mask = 1 << partsLayer;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask))
            {
                // 1. root we will actually move
                draggedPart = hit.collider.transform.parent?.gameObject;
                if (draggedPart == null) return;

                // 2. world-space point we clicked on
                Vector3 hitPoint = hit.point;

                // 3. grab point expressed in the group’s local coordinates
                grabLocal = draggedPart.transform.InverseTransformPoint(hitPoint);

                // 4. build a drag plane through that point, facing the camera
                dragPlane = new Plane(Camera.main.transform.forward, hitPoint);
            }
        }

        if (Input.GetMouseButton(0) && draggedPart != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (blockPaletteScrollRect != null)
                blockPaletteScrollRect.enabled = false;
            // 1. where the mouse ray meets that plane
            dragPlane.Raycast(ray, out float enter);
            Vector3 planePoint = ray.GetPoint(enter);

            // 2. offset between group origin and grab point (in world space)
            Vector3 grabWorldOffset = draggedPart.transform.TransformVector(grabLocal);

            // 3. desired group position so grab point == planePoint
            Vector3 target = planePoint - grabWorldOffset;

            // optional axis lock
            switch (lockedAxis)
            {
                case Axis.X: target = new Vector3(target.x, draggedPart.transform.position.y, draggedPart.transform.position.z); break;
                case Axis.Y: target = new Vector3(draggedPart.transform.position.x, target.y, draggedPart.transform.position.z); break;
                case Axis.Z: target = new Vector3(draggedPart.transform.position.x, draggedPart.transform.position.y, target.z); break;
            }

            draggedPart.transform.position = target;
        }

        if (Input.GetMouseButtonUp(0))
        {
            draggedPart = null;
            if (blockPaletteScrollRect != null)
                blockPaletteScrollRect.enabled = true;
            if (isSpawn)
            {
                HolesMode();
                isSpawn = false;
            }
        }

    }

    /* =============================================================
     *  HOVER HIGHLIGHT (disabled in PreviewAdjust)
     * =============================================================*/
    void HandleHoleHover()
    {
        if (holeState == HoleState.PreviewAdjust) return;

        GameObject hovered = GetObjectUnderCursor(holeLayer);
        if (hovered != null)
        {
            Renderer rend = hovered.GetComponent<Renderer>();
            if (rend != hoverHoleRenderer)
            {
                if (hoverHoleRenderer != null && hoverHoleRenderer != firstHoleRenderer)
                    hoverHoleRenderer.enabled = false;

                rend.enabled = true;
                hoverHoleRenderer = rend;
            }
        }
        else if (hoverHoleRenderer != null && hoverHoleRenderer != firstHoleRenderer)
        {
            hoverHoleRenderer.enabled = false;
            hoverHoleRenderer = null;
        }
    }

    GameObject GetObjectUnderCursor(int layer)
    {
        int mask = 1 << layer;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask) ? hit.collider.gameObject : null;
    }

    /* =============================================================
     *  HOLE FSM
     * =============================================================*/
    void HandleHoleFSM()
    {
        switch (holeState)
        {
            /* 1. SELECT MOVING PART’S HOLE (first click) */
            case HoleState.SelectingFirst:
                if (IsCancelPressed()) { ClearAllHoleState(); break; }
                if (IsSelectPressed())
                {
                    GameObject hit = GetObjectUnderCursor(holeLayer);
                    if (hit != null)
                    {
                        SetFirstHole(hit);
                        holeState = HoleState.SelectingSecond;
                    }
                }
                break;

            /* 2. SELECT TARGET HOLE (second click) */
            case HoleState.SelectingSecond:
                if (IsCancelPressed()) { ClearAllHoleState(); break; }
                if (IsSelectPressed())
                {
                    GameObject hit = GetObjectUnderCursor(holeLayer);
                    if (hit == null || hit == firstHoleGO) break;

                    if (AreComplementary(firstHoleGO.tag, hit.tag) && GetGroupRoot(firstHoleGO.transform) != GetGroupRoot(hit.transform))
                    {
                        secondHoleGO = hit;

                        // check if axle
                        if (firstHoleGO.CompareTag("Axle") || secondHoleGO.CompareTag("Axle"))
                        {
                            firstHoleGO = GetAxlePos(firstHoleGO.transform).gameObject;
                            secondHoleGO = GetAxlePos(secondHoleGO.transform).gameObject;
                        }
                        if (HasControlHub(firstHoleGO.transform))
                        {
                            secondHoleGO = firstHoleGO;
                            firstHoleGO = hit;
                        }

                        bool firstIsPeg = IsPeg(firstHoleGO.tag);
                        bool firstIsHole = IsHole(firstHoleGO.tag);
                        hole = firstIsHole ? firstHoleGO.transform : secondHoleGO.transform;

                        PreviewSnap(firstHoleGO, secondHoleGO);

                        if (hoverHoleRenderer != null && hoverHoleRenderer != firstHoleRenderer)
                            hoverHoleRenderer.enabled = false;
                        hoverHoleRenderer = null;

                        holeState = HoleState.PreviewAdjust;
                    }
                    else
                    {
                        SetFirstHole(hit); // restart with new moving part
                    }
                }
                break;

            /* 3. PREVIEW / ROTATE / CONFIRM */
            case HoleState.PreviewAdjust:
                HandlePreviewRotationKeys();

                if (IsCancelPressed()) { CancelPreview(true); ClearAllHoleState(); break; }
                if (IsConfirmPressed()) { CommitSnap(); break; }
                break;
        }
    }

    /* =============================================================
     *  INPUT UTILS
     * =============================================================*/
    bool IsSelectPressed() => Input.GetMouseButtonDown(0);
    bool IsConfirmPressed() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    bool IsCancelPressed() => Input.GetKeyDown(KeyCode.Escape);

    /* =============================================================
     *  SELECTION HELPERS
     * =============================================================*/
    void SetFirstHole(GameObject hole)
    {
        if (firstHoleRenderer != null) firstHoleRenderer.enabled = false;
        firstHoleGO = hole;
        firstHoleRenderer = hole.GetComponent<Renderer>();
        if (firstHoleRenderer) firstHoleRenderer.enabled = true;
        //Debug.Log($"Picked moving part: {hole.name} and group {GetGroupRoot(hole.transform).name}");
    }

    /* =============================================================
     *  PREVIEW / COMMIT
     * =============================================================*/
    void PreviewSnap(GameObject holeA, GameObject holeB)
    {
        if (movingPartRoot == null)
        {
            movingPartRoot = GetPartRoot(holeA);   // moving part root
            previewOrigPos = GetGroupRoot(movingPartRoot).position;
            previewOrigRot = GetGroupRoot(movingPartRoot).rotation;
            previewHoleRot = hole.localRotation;
        }

        // move part so holeA meets holeB (no parent change)
        SnapPartToHole(GetGroupRoot(movingPartRoot), holeA.transform, holeB.transform, true);
    }

    void CommitSnap()
    {
        Transform moverRoot = movingPartRoot;
        Transform targetRoot = GetPartRoot(secondHoleGO); // stays root

        // final align again (tiny delta) then parent AFTER animation
        SnapPartToHole(
            GetGroupRoot(movingPartRoot),
            firstHoleGO.transform,
            secondHoleGO.transform,
            true,
            () =>
            {
                MergeGroup(moverRoot, targetRoot);
                ClearAllHoleState();
            });
    }

    /* =============================================================
     *  PREVIEW ROTATION / FLIP
     * =============================================================*/
    bool IsPeg(string t) => t == "Peg" || t == "Axle";
    bool IsHole(string t) => t == "PegHole" || t == "AxleHole";

    void HandlePreviewRotationKeys()
    {
        if (firstHoleGO == null || secondHoleGO == null || movingPartRoot == null)
            return;

        // identify which root is peg vs hole
        
        //bool changed = false;
        /* ── arrow keys rotate the HOLE piece 90° ─────────────────── */
        if (hole != null)
        {
            Vector3 pivot = hole.transform.position;
            Vector3 axis = hole.transform.up;
         
            // make axis move up and down
            //if (Input.GetKeyDown(KeyCode.UpArrow)) { holeRoot.Rotate(Vector3.forward, 90f, Space.Self); changed = true; }
            //if (Input.GetKeyDown(KeyCode.DownArrow)) { holeRoot.Rotate(Vector3.forward, -90f, Space.Self); changed = true; }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                hole.RotateAround(pivot, axis, 45f);
                SnapPartToHole(GetGroupRoot(movingPartRoot),
                                   firstHoleGO.transform,
                                   secondHoleGO.transform,
                                   false);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                hole.RotateAround(pivot, axis, -45f);
                SnapPartToHole(GetGroupRoot(movingPartRoot),
                                   firstHoleGO.transform,
                                   secondHoleGO.transform,
                                   false);
            }
        }

        /* ---- F = end-over-end flip of the PEG / AXLE piece ---- */
        if (Input.GetKeyDown(KeyCode.F))
        {
            Transform pegHole = (firstHoleGO.CompareTag("PegHole") || firstHoleGO.CompareTag("AxleHole") || firstHoleGO.name.Contains("Axle"))
                        ? firstHoleGO.transform
                        : (secondHoleGO.CompareTag("PegHole") || secondHoleGO.CompareTag("AxleHole") || secondHoleGO.name.Contains("Axle"))
                          ? secondHoleGO.transform
                          : null;
 
            if (pegHole != null)
            {
                
                Vector3 pivot = pegHole.transform.position; // hole centre
                Vector3 axis = pegHole.transform.right;    // local-Right  ⟂ peg axis

                pegHole.RotateAround(pivot, axis, 180f);

                /* 3│ re-snap so holeA aligns perfectly with holeB */
                SnapPartToHole(GetGroupRoot(movingPartRoot),
                               firstHoleGO.transform,
                               secondHoleGO.transform,
                               true);   // instant, no animation
            }
        }
    }





    /* =============================================================
     *  CANCEL / RESET
     * =============================================================*/
    void ClearAllHoleState()
    {
        if (hoverHoleRenderer != null && hoverHoleRenderer != firstHoleRenderer)
            hoverHoleRenderer.enabled = false;
        if (firstHoleRenderer != null) firstHoleRenderer.enabled = false;

        hoverHoleRenderer = null;
        firstHoleRenderer = null;
        firstHoleGO = secondHoleGO = null;
        movingPartRoot = null;
        
        holeState = HoleState.SelectingFirst;
    }

    void CancelPreview(bool clearStored = false)
    {
        if (movingPartRoot != null)
        {
            GetGroupRoot(movingPartRoot).position = previewOrigPos;
            GetGroupRoot(movingPartRoot).rotation = previewOrigRot;
            hole.localRotation = previewHoleRot;
            if (clearStored) movingPartRoot = null;
        }
    }

    /* =============================================================
     *  SNAP / ALIGNMENT HELPERS
     * =============================================================*/
    bool AreComplementary(string a, string b)
    {
        return (a == "Axle" && b == "AxleHole") ||
               (a == "AxleHole" && b == "Axle") ||
               (a == "Peg" && b == "PegHole") ||
               (a == "PegHole" && b == "Peg");
    }

    bool HasControlHub(Transform selected)
    {
        Transform group = GetGroupRoot(selected);
        foreach (Transform child in group)
        {
            if (child.name == "ControlHub")
                return true;
        }
        return false;
    }
    Transform GetPartRoot(GameObject holeObj)
    {
        Transform t = holeObj.transform;
        int partsLayer = LayerMask.NameToLayer("Parts");

        while (t != null)
        {
            if (t.gameObject.layer == partsLayer)
                return t;

            t = t.parent;
        }

        return null;
    }


    Transform GetGroupRoot(Transform t)
    {
        for (int i = 0; i < 3 && t.parent != null; i++)
        {
            t = t.parent;
            if (t.name.Contains("Group"))
                return t;
        }

        return t; // No "Group" parent found within 3 levels
    }

    Transform GetAxlePos(Transform root)
    {
        foreach (Transform child in root)
        {
            if (child.name.Contains("AxlePos"))
                return child;
        }

        // No matching child found — return the original root
        return root;
    }

    Transform GetRotatingHub(Transform hole)
    {
        for (int i = 0; i < 4 && hole.parent != null; i++)
        {
            if (hole.gameObject.name == "MotorHub")
                return hole;

            hole = hole.parent;
        }

        return null;
    }

    void SnapPartToHole(
    Transform partRoot, Transform holeA, Transform holeB,
    bool animate, System.Action onComplete = null)
    {
        /* 1 – desired orientation */
        Quaternion targetRot = GetAlignment(holeA, holeB) * partRoot.rotation;

        /* 2 – vector from root → holeA, then rotate that vector */
        Vector3 fromRootToHoleA = holeA.position - partRoot.position;
        Vector3 rotatedOffset = targetRot * Quaternion.Inverse(partRoot.rotation) * fromRootToHoleA;

        /* 3 – place root so rotated holeA lands on holeB */
        Vector3 targetPos = holeB.position - rotatedOffset;

        /* 4 – apply (animate or snap) */
        if (animate)
            StartCoroutine(MovePartSmooth(partRoot, targetPos, targetRot, snapTime, onComplete));
        else
        {
            partRoot.position = targetPos;
            partRoot.rotation = targetRot;
            onComplete?.Invoke();
        }
    }



    IEnumerator MovePartSmooth(
        Transform part, Vector3 tgtPos, Quaternion tgtRot,
        float time, System.Action onComplete)
    {
        Vector3 startPos = part.position;
        Quaternion startRot = part.rotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            part.position = Vector3.Lerp(startPos, tgtPos, t);
            part.rotation = Quaternion.Slerp(startRot, tgtRot, t);
            yield return null;
        }
        onComplete?.Invoke();
    }

    Quaternion GetAlignment(Transform holeA, Transform holeB)
    {
        Quaternion from = Quaternion.LookRotation(holeA.forward, holeA.up);
        Quaternion to = Quaternion.LookRotation(holeB.forward, holeB.up);
        return to * Quaternion.Inverse(from);
    }

    /* =============================================================
    *  SPAWN PARTS FROM PART LIST
    * =============================================================*/
    public void StartDraggingSpawnedPart(GameObject prefab)
    {
        Vector3 spawnPos = GetSpawnStartPosition();
        if (currentMode == Mode.Holes)
        {   
            isSpawn = true;
            MoveMode();
        }
        
        // Create an empty container
        GameObject partRoot = new GameObject("Group");
        partRoot.transform.position = spawnPos;
        partRoot.transform.rotation = Quaternion.identity;
        partRoot.transform.SetParent(spawnRoot);

        // Spawn the actual part as a child of the container
        GameObject part = Instantiate(prefab, partRoot.transform.position, Quaternion.identity, partRoot.transform);
        part.transform.localPosition = Vector3.zero;
        part.transform.localRotation = prefab.transform.localRotation;
        part.transform.localScale = new Vector3(100,100,100);
        draggedPart = partRoot;
        // pretend they clicked the group’s origin
        Vector3 hitPoint = partRoot.transform.position;

        // local grab point is (0,0,0)
        grabLocal = Vector3.zero;

        // drag plane through that point, facing camera
        dragPlane = new Plane(Camera.main.transform.forward, hitPoint);

    }

    private Vector3 GetSpawnStartPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.point;

        // ❗ Increase this value to spawn farther from the camera
        return ray.GetPoint(spawnDistance);
    }

    /* =============================================================
    *  SET PARETING, COMBINE PARENTING MOVING
    * =============================================================*/

    void MergeGroup(Transform mover, Transform root)
    {
        Transform rotatingHub = GetRotatingHub(mover);
        Transform group = GetGroupRoot(root);

        if (rotatingHub == null) //not motor
        { 
            Transform groupA = GetGroupRoot(mover);
            Transform groupB = GetGroupRoot(root);
            Transform target, donor;
            bool AHasHub = HasControlHub(groupA);
            bool BHasHub = HasControlHub(groupB);

            if (AHasHub || BHasHub)  // has control hub, so target is controlhub
            {
                target = AHasHub ? groupA : groupB;
                donor = AHasHub ? groupB : groupA;
            }
            else
            {
                target = (groupA.childCount >= groupB.childCount) ? groupA : groupB;
                donor = (target == groupA) ? groupB : groupA;
            }
            

            // ── Move children from donor → target (keep world pose) ─────
            List<Transform> toMove = new List<Transform>();
            foreach (Transform child in donor) toMove.Add(child);

            foreach (Transform child in toMove)
                child.SetParent(target, true);   // true = keep world position

            Destroy(donor.gameObject);           // remove empty donor

            // ── Re-anchor so first child is at local (0,0,0) ─────────────
            if (target.childCount > 0)
            {
                Transform anchor = target.GetChild(0);
                Vector3 offset = anchor.localPosition;

                foreach (Transform child in target)
                    child.localPosition -= offset;

                target.position += offset;       // keep world position identical
            }
        }
        else
        {
            List<Transform> toMove = new List<Transform>();
            foreach (Transform child in group) toMove.Add(child);

            foreach (Transform child in toMove)
                child.SetParent(rotatingHub, true);
            Destroy(group.gameObject);
        }
    }
}