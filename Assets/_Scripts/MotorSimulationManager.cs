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

    public void ClearAllMotors() => motorMap.Clear();
}
