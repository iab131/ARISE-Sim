using UnityEngine;

public class StopMovingBlock : BlockBase
{
    public override void Execute(System.Action onComplete)
    {
        Debug.Log($"[{gameObject.name}] Stop all motors");
        DrivetrainController.Instance.Stop();
 
        onComplete?.Invoke();
    }

    private void StopMotors()
    {
        // TODO: Replace with your actual motor stop logic
        // For now:
        //Debug.Log("Motors stopped.");
    }
}
