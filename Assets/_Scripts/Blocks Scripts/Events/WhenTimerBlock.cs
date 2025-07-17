using UnityEngine;
using TMPro;

public class WhenTimerBlock : BlockBase, IConditionalStart
{
    public TMP_InputField timeInputField;

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

        double currentTimeSincePlay = Time.timeSinceLevelLoad - BlockGroup.playStartTime;

        //Debug.Log($"[{gameObject.name}] Checking if {currentTimeSincePlay:F2} > {threshold:F2}");

        return currentTimeSincePlay > threshold;
    }

}
