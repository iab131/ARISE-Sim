using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnAmountBlock : BlockBase, IBlockSavable
{
    [Header("Inputs")]
    public TMP_InputField directionInputField;   // e.g., "Right: 50"
    public TMP_InputField valueInputField;       // e.g., 90
    public TMP_Dropdown unitDropdown;            // "rotations", "degrees", "seconds"
    public Dictionary<string, string> SaveInputs()
    {
        return new Dictionary<string, string>
    {
        { "directionInput", directionInputField.text },
        { "value", valueInputField.text },
        { "unit", unitDropdown.value.ToString() }
    };
    }

    public void LoadInputs(Dictionary<string, string> inputs)
    {
        if (inputs.TryGetValue("directionInput", out string dir))
            directionInputField.text = dir;

        if (inputs.TryGetValue("value", out string val))
            valueInputField.text = val;

        if (inputs.TryGetValue("unit", out string unitIndex))
            unitDropdown.value = int.Parse(unitIndex);
    }

    public override void Execute(Action onComplete)
    {
        // 1. Parse directional value
        float directionalValue = ParseDirectionalInput(directionInputField.text);
        if (float.IsNaN(directionalValue))
        {
            Debug.LogWarning($"Invalid direction input on {gameObject.name}");
            onComplete?.Invoke();
            return;
        }

        // 2. Parse value
        if (!float.TryParse(valueInputField.text, out float value))
        {
            Debug.LogWarning($"Invalid value input on {gameObject.name}");
            onComplete?.Invoke();
            return;
        }

        // 3. Unit
        string unit = unitDropdown.options[unitDropdown.value].text.ToLower();
        Debug.Log($"[{gameObject.name}] Turn {directionalValue} with value {value} {unit}");

        StartCoroutine(ExecuteMotion(directionalValue,value, unit, onComplete));
    }
    private IEnumerator ExecuteMotion(float directionalValue, float value, string unit, Action onComplete)
    {

        if (DrivetrainController.Instance.NullCheck()) { onComplete?.Invoke(); yield break; }

        switch (unit)
        {
            case "rotations":
                yield return StartCoroutine(DrivetrainController.Instance.TurnForDegrees(directionalValue, value * 360));
                break;

            case "degrees":
                yield return StartCoroutine(DrivetrainController.Instance.TurnForDegrees(directionalValue, value));
                break;

            case "seconds":
                yield return StartCoroutine(DrivetrainController.Instance.TurnForSeconds(directionalValue, value));
                break;
        }

        onComplete?.Invoke();
    }
    private float ParseDirectionalInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return float.NaN;

        string[] parts = input.Split(':');
        if (parts.Length != 2) return float.NaN;

        if (!float.TryParse(parts[1].Trim(), out float value)) return float.NaN;

        return value;
    }
}
