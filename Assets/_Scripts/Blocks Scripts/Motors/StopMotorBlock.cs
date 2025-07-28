using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class StopMotorBlock : BlockBase, IBlockSavable
{
    [Header("Inputs")]
    public TMP_InputField motorInputField; // A–F (validated externally)
    public Dictionary<string, string> SaveInputs()
    {
        return new Dictionary<string, string>
    {
        { "motor", motorInputField.text }
    };
    }

    public void LoadInputs(Dictionary<string, string> inputs)
    {
        if (inputs.TryGetValue("motor", out string motor))
            motorInputField.text = motor;
    }

    public override void Execute(Action onComplete)
    {
        // 1. Get motor port
        string motorPort = motorInputField.text.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(motorPort))
        {
            Debug.LogWarning($"[{gameObject.name}] No motor port specified.");
            return;
        }

        // 2. Log and stop
        Debug.Log($"[{gameObject.name}] Stopping motor {motorPort}");

        StopMotor(motorPort);
        onComplete?.Invoke();
    }

    private void StopMotor(string port)
    {
        if (port.Length != 1 || !char.IsLetter(port[0])) { return; }
        var motor = MotorSimulationManager.Instance.GetMotor(port[0]);
        if (motor == null) { return; }
        motor.StopMotor();
    }
}
