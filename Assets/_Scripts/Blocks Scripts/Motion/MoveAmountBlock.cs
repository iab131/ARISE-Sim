using System;  // For Action
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class MoveAmountBlock : BlockBase, IBlockSavable
{
    [Header("Inputs")]
    public UpDownSelector directionSelector;
    public TMP_InputField floatInput;
    public TMP_Dropdown tmpDropdown;
    public Dictionary<string, string> SaveInputs()
    {
        return new Dictionary<string, string>
    {
        { "direction", directionSelector.CurrentDirection.ToString() },
        { "distance", floatInput.text },
        { "units", tmpDropdown.value.ToString() }
    };
    }

    public override void Execute(Action onComplete)
    {
        if (!float.TryParse(floatInput.text, out float value))
        {
            Debug.LogWarning($"Invalid numeric input on {gameObject.name}");
            onComplete?.Invoke();
            return;
        }

        string unit = tmpDropdown.options[tmpDropdown.value].text.ToLower();
        int directionMultiplier = (int)directionSelector.CurrentDirection;
        string directionText = directionSelector.CurrentDirection.ToString();
        float signedValue = value * directionMultiplier;

        Debug.Log($"[{gameObject.name}] Move {directionText} {signedValue} {unit}");

        StartCoroutine(ExecuteMotion(signedValue, unit, onComplete));
    }

    private IEnumerator ExecuteMotion(float signedValue, string unit, Action onComplete)
    {
        
        if (DrivetrainController.Instance.NullCheck()) { onComplete?.Invoke(); yield break; }

        switch (unit)
        {
            case "rotations":
                yield return StartCoroutine(DrivetrainController.Instance.DriveForDegrees(signedValue * 360));
                break;

            case "degrees":
                yield return StartCoroutine(DrivetrainController.Instance.DriveForDegrees(signedValue));
                break;

            case "seconds":
                yield return StartCoroutine(DrivetrainController.Instance.DriveForSeconds(Math.Abs(signedValue), signedValue > 0 ));
                break;
        }

        onComplete?.Invoke();
    }
    public void LoadInputs(Dictionary<string, string> inputs)
    {
        if (inputs.TryGetValue("direction", out string dir))
            directionSelector.SetDirectionFromString(dir);

        if (inputs.TryGetValue("distance", out string dist))
            floatInput.text = dist;

        if (inputs.TryGetValue("units", out string unitIndex))
            tmpDropdown.value = int.Parse(unitIndex);
    }

}
