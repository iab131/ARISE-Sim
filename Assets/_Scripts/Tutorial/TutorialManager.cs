using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Runs a sequence of TutorialStep assets. Handles UI text, highlight,
/// input gating, and completion conditions.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Steps")]
    [SerializeField] private List<TutorialStep> steps = new();

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup dimBG;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private RectTransform card;
    [SerializeField] private RectTransform highlightFrame; // outline image
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button skipBtn;
    [SerializeField] private Image raycastBlocker;         // full-screen Image (raycast target ON)

    [Header("Input (optional)")]
    [SerializeField] private PlayerInput playerInput;      // New Input System PlayerInput


    private Dictionary<string, RectTransform> uiTargets = new();
    private Dictionary<string, Transform> worldTargets = new();

    private Transform _trackedWorldTarget;
    private bool _trackWorldTarget;

    private int index;
    private Camera cam;
    private bool running;

    private const string SAVE_KEY = "tutorial.step.index";
    public static TutorialManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        cam = Camera.main;
        nextBtn.onClick.AddListener(OnNextClicked);
        backBtn.onClick.AddListener(OnBackClicked);
        skipBtn.onClick.AddListener(SkipAll);
    }

    private void Start()
    {
        // Resume from previous progress (optional)
        index = PlayerPrefs.GetInt(SAVE_KEY, 0);
        StartTutorial(index);
        //StartTutorial(12);
    }

    private void Update()
    {
        if (!_trackWorldTarget)
            return;

        UpdateWorldHighlightPosition();
    }
    /// <summary>Start or restart the tutorial from a given step index.</summary>
    public void StartTutorial(int startIndex = 0)
    {
        canvas.gameObject.SetActive(true);
        card.gameObject.SetActive(true);
        running = true;
        index = Mathf.Clamp(startIndex, 0, steps.Count - 1);
        ShowStep(index);
    }

    private void ShowStep(int i)
    {
        if (!running || i < 0 || i >= steps.Count) { EndTutorial(); return; }

        var s = steps[i];
        titleText.text = s.title;
        bodyText.text = s.body;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(card);
        // Input gating
        SetPlayerInputs(s);

        // Blocker  
        if (!string.IsNullOrEmpty(s.uiTargetName)) raycastBlocker.raycastTarget = false;
        else raycastBlocker.raycastTarget = s.blockOtherClicks;

        // Buttons availability
        nextBtn.gameObject.SetActive(s.completionMode == TutorialStep.CompletionMode.ManualNext);
        backBtn.gameObject.SetActive(i > 0);

        // Highlight
        PositionHighlight(s);

        // Save progress
        PlayerPrefs.SetInt(SAVE_KEY, i);

        // Start completion listener
        StopAllCoroutines();
        StartCoroutine(WaitForCompletion(s));
    }

    private void PositionHighlight(TutorialStep s)
    {
        highlightFrame.gameObject.SetActive(false);
        _trackWorldTarget = false;
        _trackedWorldTarget = null;

        // --- UI target highlight (static) ---
        if (!string.IsNullOrEmpty(s.uiTargetName))
        {
            RectTransform rt = ResolveUITarget(s);
            if (rt)
            {
                highlightFrame.gameObject.SetActive(true);
                highlightFrame.position = rt.TransformPoint(rt.rect.center);
                highlightFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.rect.width + 30);
                highlightFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rt.rect.height + 30);
            }
        }
        // --- World target highlight (dynamic) ---
        else if (!string.IsNullOrEmpty(s.worldTarget))
        {
            Transform wt = ResolveWorldTarget(s);
            if (wt)
            {
                highlightFrame.gameObject.SetActive(true);
                _trackedWorldTarget = wt;
                _trackWorldTarget = true;

                UpdateWorldHighlightPosition(); // position once immediately
            }
        }
    }
    private void UpdateWorldHighlightPosition()
    {
        if (!_trackedWorldTarget || !cam)
            return;

        if (!TryGetWorldBounds(_trackedWorldTarget, out Bounds bounds))
            return;

        if (!TryGetScreenRectFromBounds(bounds, out Vector3 center, out Vector2 size))
        {
            highlightFrame.gameObject.SetActive(false);
            return;
        }

        highlightFrame.gameObject.SetActive(true);
        highlightFrame.position = center;

        // Add padding so it doesn't feel tight
        highlightFrame.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, size.x + 30f);
        highlightFrame.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, size.y + 30f);
    }

    private bool TryGetScreenRectFromBounds(Bounds bounds, out Vector3 center, out Vector2 size)
    {
        Vector3[] corners = new Vector3[8];

        Vector3 ext = bounds.extents;
        Vector3 c = bounds.center;

        corners[0] = c + new Vector3(-ext.x, -ext.y, -ext.z);
        corners[1] = c + new Vector3(-ext.x, -ext.y, ext.z);
        corners[2] = c + new Vector3(-ext.x, ext.y, -ext.z);
        corners[3] = c + new Vector3(-ext.x, ext.y, ext.z);
        corners[4] = c + new Vector3(ext.x, -ext.y, -ext.z);
        corners[5] = c + new Vector3(ext.x, -ext.y, ext.z);
        corners[6] = c + new Vector3(ext.x, ext.y, -ext.z);
        corners[7] = c + new Vector3(ext.x, ext.y, ext.z);

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (var corner in corners)
        {
            Vector3 screen = cam.WorldToScreenPoint(corner);
            if (screen.z < 0)
                continue;

            min = Vector2.Min(min, screen);
            max = Vector2.Max(max, screen);
        }

        center = (min + max) * 0.5f;
        size = max - min;

        return size.x > 0 && size.y > 0;
    }

    private bool TryGetWorldBounds(Transform target, out Bounds bounds)
    {
        bounds = new Bounds();

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return false;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return true;
    }

    private IEnumerator WaitForCompletion(TutorialStep s)
    {
        switch (s.completionMode)
        {
            case TutorialStep.CompletionMode.ClickTarget:
                yield return WaitForClickOnSpecificTarget(s);
                yield return new WaitForSeconds(0.1f + s.waitSeconds);
                break;

            case TutorialStep.CompletionMode.ClickByTag:
                yield return WaitForClickByTag(s.requiredTag);
                break;

            case TutorialStep.CompletionMode.PressKey:
                yield return new WaitUntil(() => Input.GetKeyDown(s.requiredKey));
                break;

            case TutorialStep.CompletionMode.EnterTrigger:
                // Your gameplay object should call TutorialManager.SignalStepDone();
                yield return new WaitUntil(() => _signalDone);
                _signalDone = false;
                break;

            case TutorialStep.CompletionMode.Timed:
                yield return new WaitForSeconds(s.waitSeconds);
                break;

            case TutorialStep.CompletionMode.ManualNext:
                yield return new WaitUntil(() => _manualNext);
                _manualNext = false;
                break;
        }

        OnNextClicked();
    }
    private RectTransform ResolveUITarget(TutorialStep s)
    {
        if (string.IsNullOrEmpty(s.uiTargetName))
            return null;

        if (uiTargets.TryGetValue(s.uiTargetName, out var cached))
            return cached;

        var go = GameObject.Find(s.uiTargetName);
        if (!go) return null;

        var rt = go.GetComponent<RectTransform>();
        if (!rt) return null;

        uiTargets[s.uiTargetName] = rt;
        return rt;
    }
    private Transform ResolveWorldTarget(TutorialStep s)
    {
        if (string.IsNullOrEmpty(s.worldTarget))
            return null;

        if (uiTargets.TryGetValue(s.worldTarget, out var cached))
            return cached;

        var go = GameObject.Find(s.worldTarget);
        if (!go) return null;

        var rt = go.GetComponent<Transform>();
        if (!rt) return null;

        worldTargets[s.worldTarget] = rt;
        return rt;
    }
    private IEnumerator WaitForClickOnSpecificTarget(TutorialStep s)
    {
        // Allow clicking either worldTarget or uiTarget
        while (true)
        {

#if UNITY_ANDROID || UNITY_IOS
if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#else
                if (Input.GetMouseButtonDown(0))
#endif
            {
                if (IsPointerOverUI())
                {
                    // UI hit
                    RectTransform taget = ResolveUITarget(s);
                    if (taget && RectTransformUtility.RectangleContainsScreenPoint(taget, Input.mousePosition))
                    {
                        yield break;
                    }
                }
                else
                {
                    // 3D hit
                    Ray r = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(r, out var hit, Mathf.Infinity))
                    {
                        Transform taget = ResolveWorldTarget(s);
                     
                        if (taget && hit.transform == ResolveWorldTarget(s))
                            yield break;
                    }
                }
            }
            yield return null;
        }
    }
    bool IsPointerOverUI()
    {
#if UNITY_ANDROID || UNITY_IOS
    if (Input.touchCount > 0)
        return EventSystem.current.IsPointerOverGameObject(
            Input.GetTouch(0).fingerId);
    return false;
#else
        return EventSystem.current.IsPointerOverGameObject();
#endif
    }

    private IEnumerator WaitForClickByTag(string tagName)
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Ray r = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(r, out var hit, 500f))
                {
                    if (hit.transform.CompareTag(tagName)) yield break;
                }
            }
            yield return null;
        }
    }

    // ----- Input gating with New Input System -----
    private void SetPlayerInputs(TutorialStep s)
    {
        if (!playerInput)
            return;

        if (s.disableAllPlayerInputs)
        {
            playerInput.actions.Disable();
            // Re-enable specific actions if listed
            foreach (var name in s.allowActions)
            {
                var action = playerInput.actions.FindAction(name, true);
                action?.Enable();
            }
        }
        else
        {
            playerInput.actions.Enable();
        }
    }

    // ----- Public control -----
    private bool _signalDone;
    private bool _manualNext;

    /// <summary>Call from gameplay triggers to finish the current step.</summary>
    public void SignalStepDone() => _signalDone = true;

    private void OnNextClicked()
    {
        index++;
        if (index >= steps.Count) { EndTutorial(); }
        else ShowStep(index);
    }

    private void OnBackClicked()
    {
        index = Mathf.Max(0, index - 1);
        ShowStep(index);
    }

    private void SkipAll()
    {
        EndTutorial();
    }

    private void EndTutorial()
    {
        running = false;
        PlayerPrefs.SetInt(SAVE_KEY, steps.Count);
        // Hide UI and restore inputs
        dimBG.alpha = 0; dimBG.blocksRaycasts = false;
        dimBG.gameObject.SetActive(false);
        highlightFrame.gameObject.SetActive(false);
        card.gameObject.SetActive(false);
        raycastBlocker.raycastTarget = false;
        if (playerInput) playerInput.actions.Enable();
        canvas.gameObject.SetActive(false);
        gameObject.SetActive(false); // or keep and just hide
    }

    public void NotifyUIButtonClicked(string id)
    {
        if (!running) return;

        var step = steps[index];
        if (step.completionMode != TutorialStep.CompletionMode.ClickTarget)
            return;

        if (step.uiTargetName == id)
        {
            OnNextClicked();
        }
    }

}
