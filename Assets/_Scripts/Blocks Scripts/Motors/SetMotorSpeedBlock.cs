using UnityEngine;
using TMPro;

public class SetMotorSpeedBlock : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField motorInputField;      // A–F (validated by input field)
    public TMP_InputField speedInputField;      // 0–100%

    public override void Execute(System.Action onComplete)
    {
        // 1. Get motor port
        string motorPort = motorInputField.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(motorPort))
        {
            Debug.LogWarning($"[{gameObject.name}] No motor port specified.");
            return;
        }

        // 2. Parse speed input
        if (!double.TryParse(speedInputField.text, out double percent))
        {
            Debug.LogWarning($"[{gameObject.name}] Invalid speed input.");
            return;
        }

        // 3. Log and set speed
        Debug.Log($"[{gameObject.name}] Set motor {motorPort} speed to {percent}%");
        SetMotorSpeed(motorPort, percent);
        onComplete?.Invoke();
    }

    private void SetMotorSpeed(string motorPort, double percent)
    {
        // TODO: Replace with actual motor controller call
        // Example: MotorManager.SetSpeed(motorPort, percent);
        //Debug.Log($"Speed for motor {motorPort} set to {percent}%");
    }
}
