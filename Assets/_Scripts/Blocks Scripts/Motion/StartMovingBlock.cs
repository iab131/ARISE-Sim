using System;
using System.Collections.Generic;
using UnityEngine;
public class StartMovingBlock : BlockBase, IBlockSavable
{
    [Header("Inputs")]
    public UpDownSelector directionSelector;
    public Dictionary<string, string> SaveInputs()
    {
        return new Dictionary<string, string>
    {
        { "direction", directionSelector.CurrentDirection.ToString() }
    };
    }

    public void LoadInputs(Dictionary<string, string> inputs)
    {
        if (inputs.TryGetValue("direction", out string dir))
            directionSelector.SetDirectionFromString(dir);
    }

    public override void Execute(Action onComplete)
    {
        // Get direction multiplier
        int directionMultiplier = (int)directionSelector.CurrentDirection;
        string directionText = directionSelector.CurrentDirection.ToString();

        Debug.Log($"[{gameObject.name}] Move {directionText} forever");
        DrivetrainController.Instance.StartDriving(directionMultiplier > 0);
        onComplete?.Invoke();
    }
}
