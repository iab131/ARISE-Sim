using UnityEngine;

public class ConnectionActionUIManager : MonoBehaviour
{
    public static ConnectionActionUIManager Instance { get; private set; }

    private bool confirmRequested;
    private bool cancelRequested;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetVisible(false);
    }

    /// <summary>
    /// Called by ✔ UI button
    /// </summary>
    public void OnConfirmClicked()
    {
        confirmRequested = true;
    }

    /// <summary>
    /// Called by ✖ UI button
    /// </summary>
    public void OnCancelClicked()
    {
        cancelRequested = true;
    }

    /// <summary>
    /// Polled by state machine
    /// </summary>
    public bool ConsumeConfirm()
    {
        if (!confirmRequested) return false;
        confirmRequested = false;
        return true;
    }

    /// <summary>
    /// Polled by state machine
    /// </summary>
    public bool ConsumeCancel()
    {
        if (!cancelRequested) return false;
        cancelRequested = false;
        return true;
    }

    /// <summary>
    /// Optional: show/hide panel safely
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        confirmRequested = false;
        cancelRequested = false;
    }
}
