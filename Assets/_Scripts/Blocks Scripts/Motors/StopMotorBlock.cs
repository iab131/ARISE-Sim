using UnityEngine;
using TMPro;
using System;
public class StopMotorBlock : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField motorInputField; // A–F (validated externally)

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

    private void StopMotor(string motorPort)
    {
        // TODO: Replace this with your actual stop logic
        // Example: MotorManager.StopMotor(motorPort);
        //Debug.Log($"Motor {motorPort} stopped.");
    }
}
