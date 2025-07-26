using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetMoveSpeedBlock : BlockBase, IBlockSavable
{
    [Header("Inputs")]
    public TMP_InputField speedInputField;  // Accepts 0–100
    public Dictionary<string, string> SaveInputs()
    {
        return new Dictionary<string, string>
    {
        { "speed", speedInputField.text }
    };
    }

    public void LoadInputs(Dictionary<string, string> inputs)
    {
        if (inputs.TryGetValue("speed", out string speed))
            speedInputField.text = speed;
    }

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
