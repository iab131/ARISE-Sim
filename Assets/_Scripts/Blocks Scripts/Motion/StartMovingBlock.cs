using UnityEngine;
using System;
public class StartMovingBlock : BlockBase
{
    [Header("Inputs")]
    public UpDownSelector directionSelector;

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
