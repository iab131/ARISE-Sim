using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class SetMotorSpeedBlock : BlockBase, IBlockSavable
{
    [Header("Inputs")]
    public TMP_InputField motorInputField;      // A–F (validated by input field)
    public TMP_InputField speedInputField;      // 0–100%
    public Dictionary<string, string> SaveInputs()
    {
        return new Dictionary<string, string>
    {
        { "motor", motorInputField.text },
        { "speed", speedInputField.text }
    };
    }

    public void LoadInputs(Dictionary<string, string> inputs)
    {
        if (inputs.TryGetValue("motor", out string motor))
            motorInputField.text = motor;

        if (inputs.TryGetValue("speed", out string speed))
            speedInputField.text = speed;
    }

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
