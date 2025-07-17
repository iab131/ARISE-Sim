using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class PositiveInputField : MonoBehaviour
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
        if (!double.TryParse(input, out double value) || value < 0)
        {
            Debug.LogWarning("Invalid or negative number entered.");
            inputField.text = "0";
            return;
        }

        inputField.text = value.ToString("0.##");
    }
}
