using TMPro;
using UnityEngine;
using System;

public class SetMoveMotorsBlock : BlockBase
{
    [Header("Motor Inputs (A–F)")]
    public TMP_InputField leftMotorInput;
    public TMP_InputField rightMotorInput;

    public override void Execute(Action onComplete)
    {
        string leftPort = leftMotorInput.text.Trim().ToUpper();
        string rightPort = rightMotorInput.text.Trim().ToUpper();

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
        if (left.Length != 1 || !char.IsLetter(left[0])) { return; }
        if (right.Length != 1 || !char.IsLetter(right[0])) { return; }
        DrivetrainController.Instance.AssignMotorsByLabel(left[0], right[0]);
    }
}
