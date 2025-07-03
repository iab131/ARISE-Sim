using UnityEngine;
using TMPro;
using System.Collections;

public class MoveAmountBlock : BlockBase
{
    [Header("Inputs")]
    public UpDownSelector directionSelector;
    public TMP_InputField floatInput;
    public TMP_Dropdown tmpDropdown;

    public override void Execute(System.Action onComplete)
    {
        if (!double.TryParse(floatInput.text, out double value))
        {
            Debug.LogWarning($"Invalid numeric input on {gameObject.name}");
            onComplete?.Invoke();
            return;
        }

        string unit = tmpDropdown.options[tmpDropdown.value].text.ToLower();
        int directionMultiplier = (int)directionSelector.CurrentDirection;
        string directionText = directionSelector.CurrentDirection.ToString();
        double signedValue = value * directionMultiplier;

        Debug.Log($"[{gameObject.name}] Move {directionText} {signedValue} {unit}");

        switch (unit)
        {
            case "rotations":
                RotateMotor(signedValue, onComplete);
                break;
            case "degrees":
                RotateMotorByDegrees(signedValue, onComplete);
                break;
            case "seconds":
                RunMotorForSeconds(signedValue, onComplete);
                break;
            default:
                Debug.LogWarning("Unknown unit type.");
                onComplete?.Invoke();
                break;
        }
    }


    private void RotateMotor(double rotations, System.Action onComplete)
    {
        // TODO: Replace with actual motor control logic
        //Debug.Log($"[Motor] Rotating {rotations} rotations...");
        onComplete?.Invoke(); // Call immediately if not waiting
    }

    private void RotateMotorByDegrees(double degrees, System.Action onComplete)
    {
        //Debug.Log($"[Motor] Rotating {degrees} degrees...");
        onComplete?.Invoke();
    }
    private void RunMotorForSeconds(double seconds, System.Action onComplete)
    {
        StartCoroutine(WaitAndFinish((float)seconds, onComplete));
    }

    private IEnumerator WaitAndFinish(float seconds, System.Action onComplete)
    {
        //Debug.Log($"[Motor] Running for {seconds} seconds...");
        yield return new WaitForSeconds(seconds);
        //Debug.Log($"[Motor] Done.");
        onComplete?.Invoke();
    }

}
