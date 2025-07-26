using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WhenTimerBlock : BlockBase, IConditionalStart, IBlockSavable
{
    public TMP_InputField timeInputField;
    public Dictionary<string, string> SaveInputs()
    {
        return new Dictionary<string, string>
    {
        { "threshold", timeInputField.text }
    };
    }

    public void LoadInputs(Dictionary<string, string> inputs)
    {
        if (inputs.TryGetValue("threshold", out string value))
            timeInputField.text = value;
    }

    public override void Execute(System.Action onComplete)
    {
        Debug.Log($"[{gameObject.name}] Execute() triggered by timer condition.");
        // Actual execution logic happens here or via children
        onComplete?.Invoke();
    }

    public bool IsConditionMet()
    {
        if (!double.TryParse(timeInputField.text, out double threshold))
        {
            Debug.LogWarning($"[{gameObject.name}] Invalid input: '{timeInputField.text}'");
            return false;
        }

        double currentTimeSincePlay = Time.timeSinceLevelLoad - BlockCodeExecutor.playStartTime;

        //Debug.Log($"[{gameObject.name}] Checking if {currentTimeSincePlay:F2} > {threshold:F2}");

        return currentTimeSincePlay > threshold;
    }

}
