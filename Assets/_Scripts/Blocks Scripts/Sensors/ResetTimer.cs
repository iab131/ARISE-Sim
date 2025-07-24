using UnityEngine;

/// <summary>
/// A block that resets the play timer to the current time.
/// </summary>
public class ResetTimer : BlockBase
{
    public override void Execute(System.Action onComplete)
    {
        BlockCodeExecutor.playStartTime = Time.timeSinceLevelLoad;
        Debug.Log($"[{gameObject.name}] RESET TIMER — playStartTime set to {BlockCodeExecutor.playStartTime:F2}s");

        onComplete?.Invoke();
    }
}
