using System;  // For Action
using System.Collections;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
        if (!float.TryParse(valueInputField.text, out float value))
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
        float signedValue = value * directionMultiplier;

        // 5. Log
        Debug.Log($"[{gameObject.name}] Turn motor {motorPort} {rotationDirection} for {value} {unit}");

        
        StartCoroutine(ExecuteMotorRotation(motorPort, signedValue, unit, onComplete));

        // 6. Handle unit logic
        //switch (unit)
        //{
        //    case "rotations":
        //        RotateMotor(motorPort, signedValue);  
        //        onComplete?.Invoke();
        //        break;

        //    case "degrees":
        //        RotateMotorByDegrees(motorPort, signedValue);
        //        onComplete?.Invoke();
        //        break;

        //    case "seconds":
        //        RunMotorForSeconds(motorPort, signedValue, onComplete);
        //        break;

        //    default:
        //        Debug.LogWarning("Unknown unit type.");
        //        onComplete?.Invoke();
        //        break;
        //}
    }
    private IEnumerator ExecuteMotorRotation(string port, float signedValue, string unit, Action onComplete)
    {
        if (port.Length != 1 || !char.IsLetter(port[0])) { onComplete?.Invoke(); yield break; }
        var motor = MotorSimulationManager.Instance.GetMotor(port[0]);
        if (motor == null) { onComplete?.Invoke(); yield break; }

        switch (unit)
        {
            case "rotations":
                yield return motor.RotateByDegrees(signedValue * 360f);
                break;

            case "degrees":
                yield return motor.RotateByDegrees(signedValue);
                break;

            case "seconds":
                yield return motor.RunForSeconds(signedValue);
                break;
        }

        onComplete?.Invoke();
    }

    private void RotateMotor(string port, float rotations)
    {
        if (port.Length != 1 || !char.IsLetter(port[0])) return;
        var motor = MotorSimulationManager.Instance.GetMotor(port[0]);
        motor?.RotateRotations(rotations);
    }

    private void RotateMotorByDegrees(string port, float degrees)
    {
        if (port.Length != 1 || !char.IsLetter(port[0])) return;
        var motor = MotorSimulationManager.Instance.GetMotor(port[0]);
        motor?.RotateByDegrees(degrees);
        Debug.Log(degrees);
    }

    private void RunMotorForSeconds(string port, double seconds, Action onComplete)
    {
        if (port.Length != 1 || !char.IsLetter(port[0])) { onComplete?.Invoke(); return; }
        var motor = MotorSimulationManager.Instance.GetMotor(port[0]);
        if (motor != null)
            motor.RunForSeconds((float)seconds);
        onComplete?.Invoke();
    }


}
