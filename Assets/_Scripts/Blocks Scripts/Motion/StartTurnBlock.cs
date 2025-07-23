using UnityEngine;
using TMPro;
using System;
public class StartTurnBlock : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField directionInputField;  // Format: "Right: 40", "Left: -30"

    public override void Execute(Action onComplete)
    {
        float signedValue = ParseDirectionalInput(directionInputField.text);
        if (float.IsNaN(signedValue))
        {
            Debug.LogWarning($"Invalid direction input on {gameObject.name}");
            return;
        }

        string directionText = signedValue >= 0 ? "Right" : "Left";

        Debug.Log($"[{gameObject.name}] Start moving {directionText} forever with input value {signedValue}");

        DrivetrainController.Instance.StartTurning(signedValue);

        onComplete?.Invoke();
    }

    private float ParseDirectionalInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return float.NaN;

        string[] parts = input.Split(':');
        if (parts.Length != 2) return float.NaN;

        string valueStr = parts[1].Trim();
        if (!float.TryParse(valueStr, out float value)) return float.NaN;

        return value;
    }
}
