using UnityEngine;

/// <summary>
/// Two-finger rotate for AR mat alignment.
/// </summary>
public class ARRotate : MonoBehaviour
{
    public float rotationSpeed = 0.3f;

    void Update()
    {
        if (Input.touchCount != 2) return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 prevDir = (t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition);
        Vector2 currDir = t0.position - t1.position;

        float angle = Vector2.SignedAngle(prevDir, currDir);
        transform.Rotate(Vector3.up, angle * rotationSpeed);
    }
}
