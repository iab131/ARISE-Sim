using UnityEngine;
using TMPro;

public class SetMoveMotorsBlock : BlockBase
{
    [Header("Motor Inputs (A–F)")]
    public TMP_InputField leftMotorInput;
    public TMP_InputField rightMotorInput;

    public override void Execute(System.Action onComplete)
    {
        string leftPort = leftMotorInput.text;
        string rightPort = rightMotorInput.text;

        if (string.IsNullOrWhiteSpace(leftPort) || string.IsNullOrWhiteSpace(rightPort))
        {
            Debug.LogWarning($"[{gameObject.name}] Invalid motor port(s).");
            return;
        }

        Debug.Log($"[{gameObject.name}] Assigned motors → Left: {leftPort}, Right: {rightPort}");

        SetMotors(leftPort, rightPort);
        onComplete?.Invoke();
    }

    private void SetMotors(string left, string right)
    {
        // TODO: Connect to your motor controller logic
        // Example: MotorManager.AssignDriveMotors(left, right);
        //Debug.Log($"Motors set → Left: {left}, Right: {right}");
    }
}
