using System;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class SetMotorSpeedBlock : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField motorInputField;      // A–F (validated by input field)
    public TMP_InputField speedInputField;      // 0–100%

    public override void Execute(Action onComplete)
    {
        // 1. Get motor port
        string motorPort = motorInputField.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(motorPort))
        {
            Debug.LogWarning($"[{gameObject.name}] No motor port specified.");
            return;
        }

        // 2. Parse speed input
        if (!float.TryParse(speedInputField.text, out float percent))
        {
            Debug.LogWarning($"[{gameObject.name}] Invalid speed input.");
            return;
        }

        // 3. Log and set speed
        Debug.Log($"[{gameObject.name}] Set motor {motorPort} speed to {percent}%");
        SetMotorSpeed(motorPort, percent);
        onComplete?.Invoke();
    }

    private void SetMotorSpeed(string port, float percent)
    {
        if (port.Length != 1 || !char.IsLetter(port[0])) { return; }
        var motor = MotorSimulationManager.Instance.GetMotor(port[0]);
        if (motor == null) { return; }
        motor.SetSpeed(percent);
    }
}
