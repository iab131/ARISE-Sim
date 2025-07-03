using UnityEngine;

public class StartMovingBlock : BlockBase
{
    [Header("Inputs")]
    public UpDownSelector directionSelector;

    public override void Execute(System.Action onComplete)
    {
        // Get direction multiplier
        int directionMultiplier = (int)directionSelector.CurrentDirection;
        string directionText = directionSelector.CurrentDirection.ToString();

        Debug.Log($"[{gameObject.name}] Move {directionText} forever");

        RunMotorForever(directionMultiplier);
        onComplete?.Invoke();
    }

    private void RunMotorForever(int direction)
    {
        // direction = +1 or -1
        // TODO: replace with actual motor code
        //Debug.Log($"Motor running {(direction > 0 ? "forward" : "backward")} indefinitely...");
    }
}
