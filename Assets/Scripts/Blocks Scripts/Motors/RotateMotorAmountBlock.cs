using UnityEngine;
using TMPro;
using System;  // For Action
using System.Collections;

public class TurnMotorAmountBlock : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField motorInputField;        // A–F
    public UpDownSelector directionSelector;      // Forward/Backward
    public TMP_InputField valueInputField;        // Numeric value
    public TMP_Dropdown unitDropdown;             // Unit: rotations, degrees, seconds

    public override void Execute(Action onComplete)
    {
        // 1. Parse motor port
        string motorPort = motorInputField.text.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(motorPort))
        {
            Debug.LogWarning($"[{gameObject.name}] Invalid motor port.");
            onComplete?.Invoke();
            return;
        }

        // 2. Parse value
        if (!double.TryParse(valueInputField.text, out double value))
        {
            Debug.LogWarning($"[{gameObject.name}] Invalid value input.");
            onComplete?.Invoke();
            return;
        }

        // 3. Get direction and unit
        int directionMultiplier = (int)directionSelector.CurrentDirection;
        string rotationDirection = directionSelector.CurrentDirection == UpDownSelector.Direction.Forward
            ? "clockwise"
            : "counterclockwise";
        string unit = unitDropdown.options[unitDropdown.value].text.ToLower();

        // 4. Calculate signed value
        double signedValue = value * directionMultiplier;

        // 5. Log
        Debug.Log($"[{gameObject.name}] Turn motor {motorPort} {rotationDirection} for {value} {unit}");

        // 6. Handle unit logic
        switch (unit)
        {
            case "rotations":
                RotateMotor(motorPort, signedValue);
                onComplete?.Invoke();
                break;

            case "degrees":
                RotateMotorByDegrees(motorPort, signedValue);
                onComplete?.Invoke();
                break;

            case "seconds":
                RunMotorForSeconds(motorPort, signedValue, onComplete);
                break;

            default:
                Debug.LogWarning("Unknown unit type.");
                onComplete?.Invoke();
                break;
        }
    }

    private void RotateMotor(string port, double rotations)
    {
        // TODO: Hook into motor manager
        //Debug.Log($"[Motor] Rotate {port} for {rotations} rotations");
    }

    private void RotateMotorByDegrees(string port, double degrees)
    {
        // TODO: Hook into motor manager
        //Debug.Log($"[Motor] Rotate {port} for {degrees} degrees");
    }

    private void RunMotorForSeconds(string port, double seconds, Action onComplete)
    {
        //Debug.Log($"[Motor] Run {port} for {seconds} seconds");
        StartCoroutine(WaitAndFinish((float)Mathf.Abs((float)seconds), onComplete));
    }

    private IEnumerator WaitAndFinish(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        //Debug.Log("[Motor] Done waiting.");
        onComplete?.Invoke();
    }
}
