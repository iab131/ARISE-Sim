using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetMoveMotorsBlock : BlockBase, IBlockSavable
{
    [Header("Motor Inputs (A–F)")]
    public TMP_InputField leftMotorInput;
    public TMP_InputField rightMotorInput;

    public Dictionary<string, string> SaveInputs()
    {
        return new Dictionary<string, string>
    {
        { "leftMotor", leftMotorInput.text },
        { "rightMotor", rightMotorInput.text }
    };
    }

    public void LoadInputs(Dictionary<string, string> inputs)
    {
        if (inputs.TryGetValue("leftMotor", out string left))
            leftMotorInput.text = left;

        if (inputs.TryGetValue("rightMotor", out string right))
            rightMotorInput.text = right;
    }

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
