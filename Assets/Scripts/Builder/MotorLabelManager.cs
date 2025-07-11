using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;   // ← needed for PhysicsRaycaster
using TMPro;
using System.Collections.Generic;

public class MotorLabelManager : MonoBehaviour
{
    // ───────── Singleton ─────────
    public static MotorLabelManager Instance { get; private set; }

    // ───────── UI references ─────────
    [Header("UI")]
    [SerializeField] private Button setMotorButton;
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private TextMeshProUGUI motorNumberText;
    [Tooltip("Six buttons labelled A–F, index 0 = A … 5 = F")]
    [SerializeField] private Button[] letterButtons = new Button[6];
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    // ───────── Scene references ─────────
    [Header("Scene")]
    [Tooltip("Main-camera PhysicsRaycaster that drives all 3-D OnMouse events.")]
    [SerializeField] private PhysicsRaycaster camRaycaster;   // drag MainCamera’s component here

    [Header("Colours")]
    [SerializeField] private Color normalColour = Color.white;
    [SerializeField] private Color disabledColour = new(.6f, .6f, .6f, 1f);
    [SerializeField] private Color selectedColour = new(.3f, .8f, 1f, 1f);

    // ───────── Runtime ─────────
    public bool assignModeActive { get; private set; }
    private MotorLabel currentTarget;
    private readonly Dictionary<char, MotorLabel> takenLetters = new();
    private char previewLetter = '\0';

    // ───────── Unity life-cycle ─────────
    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (camRaycaster == null)                        // auto-grab if you forgot
            camRaycaster = FindAnyObjectByType<PhysicsRaycaster>();

        modalPanel.SetActive(false);

        setMotorButton.onClick.AddListener(ToggleAssignMode);

        // A–F buttons
        for (int i = 0; i < letterButtons.Length; ++i)
        {
            int idx = i;
            letterButtons[i].onClick.AddListener(() => OnLetterPreview((char)('A' + idx)));
        }

        confirmButton.onClick.AddListener(ConfirmSelection);
        cancelButton.onClick.AddListener(CancelSelection);
    }

    // ───────── called from MotorLabelBrickRelay ─────────
    public bool IsModalOpen => modalPanel.activeSelf;

    public void OnMotorClicked(MotorLabel clicked)
    {
        if (!assignModeActive) return;

        if (currentTarget) currentTarget.SetHovered(false);
        currentTarget = clicked;
        currentTarget.SetHovered(true);

        motorNumberText.text = $"Motor #{currentTarget.MotorIndex}";
        previewLetter = '\0';
        PrepareLetterButtons();

        // — disable world raycast while modal is open
        if (camRaycaster) camRaycaster.enabled = false;
        modalPanel.SetActive(true);
    }

    // ───────── Toggle assign-mode from button or API ─────────
    private void ToggleAssignMode() => SetAssignMode(!assignModeActive);

    /** Public API — other systems can call this to enter / exit label mode */
    public void SetAssignMode(bool enable)
    {
        if (enable == assignModeActive) return;   // nothing to do
        assignModeActive = enable;

        if (!assignModeActive) CloseModal();      // hides modal & hover

        foreach (MotorLabel lbl in Object.FindObjectsByType<MotorLabel>(FindObjectsSortMode.None))
        {
            lbl.SetLabelVisible(assignModeActive);
            if (assignModeActive) lbl.AdjustLabelToAvoidCollision();
            else lbl.SetHovered(false);
        }
    }

    // ───────── preview ─────────
    private void OnLetterPreview(char letter)
    {
        if (!currentTarget) return;
        if (takenLetters.TryGetValue(letter, out MotorLabel owner) && owner != currentTarget)
            return;                                // already taken by someone else

        previewLetter = letter;
        currentTarget.SetLetter(letter);
        HighlightSelectedButton(letter);
    }

    // ───────── confirm / cancel ─────────
    private void ConfirmSelection()
    {
        if (!currentTarget || previewLetter == '\0') { CloseModal(); return; }

        // release any previous claim
        foreach (var kv in takenLetters)
            if (kv.Value == currentTarget) { takenLetters.Remove(kv.Key); break; }

        takenLetters[previewLetter] = currentTarget;
        CloseModal();
    }

    private void CancelSelection()
    {
        if (currentTarget)
        {
            char committed = '?';
            foreach (var kv in takenLetters)
                if (kv.Value == currentTarget) { committed = kv.Key; break; }
            currentTarget.SetLetter(committed);
            currentTarget.SetHovered(false);
        }
        CloseModal();
    }

    // ───────── helpers ─────────
    private void PrepareLetterButtons()
    {
        for (int i = 0; i < letterButtons.Length; ++i)
        {
            char letter = (char)('A' + i);
            bool taken = takenLetters.ContainsKey(letter) && takenLetters[letter] != currentTarget;

            Button b = letterButtons[i];
            b.interactable = !taken;
            b.image.color = taken ? disabledColour : normalColour;
        }
        confirmButton.interactable = false;
    }

    private void HighlightSelectedButton(char letter)
    {
        for (int i = 0; i < letterButtons.Length; ++i)
        {
            Button b = letterButtons[i];
            if ((char)('A' + i) == letter && b.interactable)
            {
                b.image.color = selectedColour;
                confirmButton.interactable = true;
            }
            else if (b.interactable) b.image.color = normalColour;
        }
    }

    private void CloseModal()
    {
        modalPanel.SetActive(false);
        if (camRaycaster) camRaycaster.enabled = true;   // re-enable 3-D clicks

        previewLetter = '\0';
        if (currentTarget) currentTarget.SetHovered(false);
        currentTarget = null;
    }
}
