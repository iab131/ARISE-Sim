using UnityEngine;
using TMPro;
using System.Collections;
using System;  // For Action
public class MoveAmountBlock : BlockBase
{
    [Header("Inputs")]
    public UpDownSelector directionSelector;
    public TMP_InputField floatInput;
    public TMP_Dropdown tmpDropdown;

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

}
