using UnityEngine;

public abstract class BlockBase : MonoBehaviour
{
    /// <summary>
    /// Executes the block logic and invokes the callback when done.
    /// </summary>
    public virtual void Execute(System.Action onComplete)
    {
        onComplete?.Invoke(); // Default: immediate execution
    }
}
