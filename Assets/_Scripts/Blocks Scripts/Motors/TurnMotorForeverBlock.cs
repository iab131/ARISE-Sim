using UnityEngine;
using TMPro;
using System;
public class TurnMotorForeverBlock : BlockBase
{
    [Header("Inputs")]
    public TMP_InputField motorInputField;        // e.g., A–F
    public UpDownSelector directionSelector;      // Forward / Backward

    public override void Execute(Action onComplete)
    {
        // 1. Get motor port (already validated by input field)
        string motorPort = motorInputField.text.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(motorPort))
        {
            Debug.LogWarning($"[{gameObject.name}] Invalid motor port.");
            return;
        }
        // 2. Get direction
        int directionMultiplier = (int)directionSelector.CurrentDirection;
        string rotationDirection = directionSelector.CurrentDirection == UpDownSelector.Direction.Forward
            ? "clockwise"
            : "counterclockwise";

        // 3. Log
        Debug.Log($"[{gameObject.name}] Run motor {motorPort} {rotationDirection} forever");

        // 4. Execute
        RunMotorForever(motorPort, directionMultiplier);
        onComplete?.Invoke();
    }

    private void RunMotorForever(string motorPort, int direction)
    {
        // direction = +1 (Forward) or -1 (Backward)
        // TODO: Replace with actual motor control logic
        // Example: MotorManager.RunForever(motorPort, direction);
        //Debug.Log($"Motor {motorPort} running forever in direction {direction}");
    }
}
