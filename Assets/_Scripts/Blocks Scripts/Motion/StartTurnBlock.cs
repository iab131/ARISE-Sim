using UnityEngine;
using TMPro;

public class StartTurnBlockop : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField directionInputField;  // Format: "Right: 40", "Left: -30"

    public override void Execute(System.Action onComplete)
    {
        double signedValue = ParseDirectionalInput(directionInputField.text);
        if (double.IsNaN(signedValue))
        {
            Debug.LogWarning($"Invalid direction input on {gameObject.name}");
            return;
        }

        string directionText = signedValue >= 0 ? "Right" : "Left";

        Debug.Log($"[{gameObject.name}] Start moving {directionText} forever with input value {signedValue}");

        RunMotorForever(signedValue);
        onComplete?.Invoke();
    }

    private double ParseDirectionalInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return double.NaN;

        string[] parts = input.Split(':');
        if (parts.Length != 2) return double.NaN;

        string valueStr = parts[1].Trim();
        if (!double.TryParse(valueStr, out double value)) return double.NaN;

        return value;
    }

    private void RunMotorForever(double signedValue)
    {
        // TODO: replace with real motor logic
        //Debug.Log($"Running motor indefinitely with input {signedValue}");
    }
}
