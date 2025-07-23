using UnityEngine;
using TMPro;
using System;

public class SetMoveSpeedBlock : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField speedInputField;  // Accepts 0–100

    public override void Execute(Action onComplete)
    {
        // Parse input
        if (!float.TryParse(speedInputField.text, out float speedPercent))
        {
            Debug.LogWarning($"Invalid speed input on {gameObject.name}");
            return;
        }


        Debug.Log($"[{gameObject.name}] Set motor speed to {speedPercent}%");

        SetMotorSpeed(speedPercent);
        onComplete?.Invoke();
    }

    private void SetMotorSpeed(float percent)
    {
        DrivetrainController.Instance.SetSpeedPercent(percent);
    }
}
