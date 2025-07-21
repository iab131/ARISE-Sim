using UnityEngine;

public class StopAllBlock : BlockBase
{
    public override void Execute(System.Action onComplete)
    {
        Debug.Log($"[{gameObject.name}] STOP ALL — ending block execution.");
        BlockCodeExecutor.StopExecution();
        // ⚠ Do NOT call onComplete — stops the chain
    }
}
