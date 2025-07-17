using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class PercentInputField : MonoBehaviour
{
    public TMP_InputField inputField;

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();

        inputField.onEndEdit.AddListener(OnInputEnd);
    }

    private void OnInputEnd(string input)
    {
        // Try parsing
        if (!double.TryParse(input, out double value))
        {
            Debug.LogWarning("Invalid number entered.");
            inputField.text = "";
            return;
        }

        // Clamp between 0–100
        value = Mathf.Clamp((float)value, 0f, 100f);

        // Show just the number (max 2 decimal places)
        inputField.text = value.ToString("0.##");
    }
}
