using UnityEngine;
using TMPro;
using System.Collections;

public class WaitSecondsBlock : BlockBase
{
    public TMP_InputField secondsInputField;

    public override void Execute(System.Action onComplete)
    {
        if (!double.TryParse(secondsInputField.text, out double seconds) || seconds < 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Invalid wait time input.");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"[{gameObject.name}] Waiting {seconds} seconds...");
        StartCoroutine(WaitAndContinue((float)seconds, onComplete));
    }

    private IEnumerator WaitAndContinue(float seconds, System.Action onComplete)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log($"[{gameObject.name}] Done waiting.");
        onComplete?.Invoke();
    }
}

