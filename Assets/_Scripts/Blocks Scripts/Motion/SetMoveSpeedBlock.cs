using UnityEngine;
using TMPro;

public class SetMoveSpeedBlock : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField speedInputField;  // Accepts 0–100

    public override void Execute(System.Action onComplete)
    {
        // Parse input
        if (!double.TryParse(speedInputField.text, out double speedPercent))
        {
            Debug.LogWarning($"Invalid speed input on {gameObject.name}");
            return;
        }


        Debug.Log($"[{gameObject.name}] Set motor speed to {speedPercent}%");

        SetMotorSpeed(speedPercent);
        onComplete?.Invoke();
    }

    private void SetMotorSpeed(double percent)
    {
        // TODO: Replace with real motor control logic
        // Example: MotorManager.SetGlobalSpeed(percent);
        //Debug.Log($"Motor speed set to {percent}%");
    }
}
