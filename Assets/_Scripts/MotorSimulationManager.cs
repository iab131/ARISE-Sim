using System.Collections.Generic;
using UnityEngine;

public class MotorSimulationManager : MonoBehaviour
{
    public static MotorSimulationManager Instance { get; private set; }

    private Dictionary<char, SimMotor> motorMap = new();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void RegisterMotor(char label, SimMotor motor)
    {
        motorMap[label] = motor;
    }

    public SimMotor GetMotor(char label)
    {
        motorMap.TryGetValue(label, out var motor);
        return motor;
    }

    public void ClearAllMotors()
    {
        motorMap.Clear();
    }

    public void StopAllMotors()
    {
        foreach(var motor in motorMap.Values)
        {
            motor.StopMotor();
        }
        DrivetrainController.Instance.Stop();
    }
    /// <summary>
    /// Removes the given motor from the dictionary (no StopMotor call).
    /// </summary>
    public void ClearMotor(SimMotor motor)
    {
        if (motor == null) return;

        char keyToRemove = '\0';

        // Find the key for this motor
        foreach (var kv in motorMap)
        {
            if (kv.Value == motor)
            {
                keyToRemove = kv.Key;
                break;
            }
        }

        if (keyToRemove != '\0')
        {
            motorMap.Remove(keyToRemove);
        }
    }

}
