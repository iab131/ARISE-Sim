using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class MotorPortInputField : MonoBehaviour
{
    public TMP_InputField inputField;

    [Tooltip("Link to the other motor input field to check for duplicates")]
    public MotorPortInputField otherMotorInput;

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();

        inputField.onEndEdit.AddListener(OnInputEnd);
    }

    private void OnInputEnd(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            inputField.text = "";
            return;
        }

        // Trim and uppercase
        string letter = input.Trim().ToUpper();

        // Validate it's A to F
        if (!"ABCDEF".Contains(letter) || letter.Length != 1)
        {
            inputField.text = "";
            return;
        }

        // Check if it matches the other motor field
        if (otherMotorInput != null && otherMotorInput.GetMotorLetter() == letter)
        {
            inputField.text = "";
            return;
        }

        inputField.text = letter;
    }

    public string GetMotorLetter()
    {
        return inputField.text.Trim().ToUpper();
    }
}
