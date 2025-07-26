using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaitSecondsBlock : BlockBase, IBlockSavable
{
    public TMP_InputField secondsInputField;
    public Dictionary<string, string> SaveInputs()
    {
        return new Dictionary<string, string>
    {
        { "seconds", secondsInputField.text }
    };
    }

    public void LoadInputs(Dictionary<string, string> inputs)
    {
        if (inputs.TryGetValue("seconds", out string value))
            secondsInputField.text = value;
    }

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

