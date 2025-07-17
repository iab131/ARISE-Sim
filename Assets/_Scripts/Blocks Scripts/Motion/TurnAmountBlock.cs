using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class TurnAmountBlock : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField directionInputField;   // e.g., "Right: 50"
    public TMP_InputField valueInputField;       // e.g., 90
    public TMP_Dropdown unitDropdown;            // "rotations", "degrees", "seconds"

    public override void Execute(Action onComplete)
    {
        // 1. Parse directional value
        double signedValue = ParseDirectionalInput(directionInputField.text);
        if (double.IsNaN(signedValue))
        {
            Debug.LogWarning($"Invalid direction input on {gameObject.name}");
            onComplete?.Invoke();
            return;
        }

        // 2. Parse value
        if (!double.TryParse(valueInputField.text, out double value))
        {
            Debug.LogWarning($"Invalid value input on {gameObject.name}");
            onComplete?.Invoke();
            return;
        }

        // 3. Unit
        string unit = unitDropdown.options[unitDropdown.value].text.ToLower();
        Debug.Log($"[{gameObject.name}] Turn {signedValue} with value {value} {unit}");

        // 4. Dispatch by unit
        switch (unit)
        {
            case "rotations":
                RotateMotor(signedValue, value, onComplete);
                break;
            case "degrees":
                RotateMotorByDegrees(signedValue, value, onComplete);
                break;
            case "seconds":
                RunMotorForSeconds(signedValue, value, onComplete);
                break;
            default:
                Debug.LogWarning("Unknown unit type.");
                onComplete?.Invoke();
                break;
        }
    }

    private double ParseDirectionalInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return double.NaN;

        string[] parts = input.Split(':');
        if (parts.Length != 2) return double.NaN;

        if (!double.TryParse(parts[1].Trim(), out double value)) return double.NaN;

        return value;
    }

    // Placeholder functions

    private void RotateMotor(double directionValue, double rotations, Action onComplete)
    {
        Debug.Log($"[Motor] Rotate {directionValue} for {rotations} rotations");
        onComplete?.Invoke(); // replace with real async call if needed
    }

    private void RotateMotorByDegrees(double directionValue, double degrees, Action onComplete)
    {
        Debug.Log($"[Motor] Rotate {directionValue} for {degrees} degrees");
        onComplete?.Invoke();
    }

    private void RunMotorForSeconds(double directionValue, double seconds, Action onComplete)
    {
        Debug.Log($"[Motor] Rotate {directionValue} for {seconds} seconds");
        StartCoroutine(WaitAndComplete((float)seconds, onComplete));
    }

    private IEnumerator WaitAndComplete(float seconds, Action done)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log($"[Motor] Done waiting {seconds} seconds.");
        done?.Invoke();
    }
}
