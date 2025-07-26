using UnityEngine;

public class StopMovingBlock : BlockBase
{
    public override void Execute(System.Action onComplete)
    {
        Debug.Log($"[{gameObject.name}] Stop all motors");
        DrivetrainController.Instance.Stop();
 
        onComplete?.Invoke();
    }

}
