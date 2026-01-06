using UnityEngine;
using TMPro;

/// <summary>
/// One step of the tutorial (author in Project as assets).
/// </summary>
[CreateAssetMenu(menuName = "Tutorial/Step", fileName = "TutorialStep")]
public class TutorialStep : ScriptableObject
{
    [Header("Copy")]
    [TextArea] public string title;
    [TextArea] public string body;

    [Header("Target / Highlight")]
    public string worldTarget;                 // Optional 3D object to highlight
    public string uiTargetName;
    public bool blockOtherClicks = true;          // Block all input except target?

    [Header("Completion")]
    public CompletionMode completionMode = CompletionMode.ClickTarget;
    public string requiredTag = "";               // e.g., "PlayButton" for ClickByTag
    public KeyCode requiredKey = KeyCode.None;    // e.g., Space to continue
    public float waitSeconds = 0f;                // Timed steps

    [Header("Input Gating")]
    public bool disableAllPlayerInputs = true;    // Turn off your Player Input
    public string[] allowActions = new string[0]; // New Input System action names to allow

    public enum CompletionMode
    {
        ClickTarget,      // Left click on target
        ClickByTag,       // Left click any object with tag
        PressKey,         // Press specific key
        EnterTrigger,     // Some trigger tells us done
        Timed,            // waitSeconds
        ManualNext        // Next button only
    }
}
