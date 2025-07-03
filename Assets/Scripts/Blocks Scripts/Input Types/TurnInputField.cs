using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class TurnInputField : MonoBehaviour
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
            inputField.text = ""; // Clear on invalid
            return;
        }

        // Clamp range
        value = Mathf.Clamp((float)value, -100f, 100f); // float.Clamp is fine even for double range

        // Format text
        string direction = value >= 0 ? "right" : "left";
        inputField.text = $"{direction}: {(float)value:0.##}";
    }
}
