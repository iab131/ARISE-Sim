using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MotorHubPhysicsToggle : MonoBehaviour
{
    private Rigidbody rb;
    private static List<MotorHubPhysicsToggle> allHubs = new List<MotorHubPhysicsToggle>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        allHubs.Add(this);
    }

    private void OnDestroy()
    {
        allHubs.Remove(this);
    }

    public void SetBuildMode(bool isBuilding)
    {
        rb.isKinematic = isBuilding;
        rb.interpolation = isBuilding ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
    }

    /// <summary>
    /// Call this to toggle all motorhubs at once
    /// </summary>
    public static void SetBuildModeForAll(bool isBuilding)
    {
        foreach (var hub in allHubs)
        {
            hub.SetBuildMode(isBuilding);
        }
    }
}
