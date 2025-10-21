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

    private int index;
    private Camera cam;
    private bool running;

    private const string SAVE_KEY = "tutorial.step.index";

    private void Awake()
    {
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
    }

    /// <summary>Start or restart the tutorial from a given step index.</summary>
    public void StartTutorial(int startIndex = 0)
    {
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

        // Input gating
        SetPlayerInputs(s);

        // Blocker
        raycastBlocker.raycastTarget = s.blockOtherClicks;

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

        if (s.uiTarget)
        {
            highlightFrame.gameObject.SetActive(true);
            highlightFrame.position = s.uiTarget.TransformPoint(s.uiTarget.rect.center);
            highlightFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.uiTarget.rect.width + 12);
            highlightFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s.uiTarget.rect.height + 12);
        }
        else if (s.worldTarget)
        {
            Vector3 screen = cam.WorldToScreenPoint(s.worldTarget.position);
            highlightFrame.gameObject.SetActive(true);
            highlightFrame.position = screen;
            // Optional: size by renderer bounds → convert bounds to screen size
        }
    }

    private IEnumerator WaitForCompletion(TutorialStep s)
    {
        switch (s.completionMode)
        {
            case TutorialStep.CompletionMode.ClickTarget:
                yield return WaitForClickOnSpecificTarget(s);
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

    private IEnumerator WaitForClickOnSpecificTarget(TutorialStep s)
    {
        // Allow clicking either worldTarget or uiTarget
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    // UI hit
                    if (s.uiTarget && RectTransformUtility.RectangleContainsScreenPoint(s.uiTarget, Input.mousePosition))
                        yield break;
                }
                else
                {
                    // 3D hit
                    Ray r = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(r, out var hit, 500f))
                    {
                        if (s.worldTarget && hit.transform == s.worldTarget)
                            yield break;
                    }
                }
            }
            yield return null;
        }
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
        highlightFrame.gameObject.SetActive(false);
        raycastBlocker.raycastTarget = false;
        if (playerInput) playerInput.actions.Enable();
        gameObject.SetActive(false); // or keep and just hide
    }
}
