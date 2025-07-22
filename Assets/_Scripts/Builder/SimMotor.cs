using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(HingeJoint))]
public class SimMotor : MonoBehaviour
{
    // ───────── Static Registry ─────────
    private static List<SimMotor> allMotors = new();
    public static void SetBuildModeForAll(bool isBuilding)
    {
        foreach (var m in allMotors) m.SetBuildMode(isBuilding);
    }
    public static void SetConnectedBodyForAll(Rigidbody body)
    {
        foreach (var m in allMotors) m.motor.connectedBody = body;
    }

    // ───────── Inspector / Label ─────────
    public char motorLabel = '\0';  // e.g. 'A', 'B', etc.
    
    public float speed = 1000f;
    private const float MAX_SPEED = 1000f;
    private const float FORCE = 150f;
    private Rigidbody rb;
    private HingeJoint motor;

    private float kP = 20f;
    private float kD = 0.5f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        motor = GetComponent<HingeJoint>();

        if (motorLabel != '\0')
        {
            MotorSimulationManager.Instance.RegisterMotor(motorLabel, this);
        }
        allMotors.Add(this);

        JointMotor m = motor.motor;
        m.force = FORCE;
        m.targetVelocity = 0f;
        m.freeSpin = false;
        motor.motor = m;
        motor.useMotor = true;
    }

    private void OnDestroy()
    {
        allMotors.Remove(this);
    }
    public void SetMotorLabel(char label)
    {
        motorLabel = label;
        MotorSimulationManager.Instance.RegisterMotor(motorLabel, this);
    }
    // ───────── Simulation Control ─────────
    public void SetBuildMode(bool isBuilding)
    {
        rb.isKinematic = isBuilding;
        //rb.interpolation = isBuilding ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
    }

    public Coroutine RotateRotations(float rotations)
    {
        return RotateByDegrees(rotations * 360f);
    }

    public Coroutine RotateByDegrees(float degrees)
    {
        return StartCoroutine(RotateToAnglePID(degrees));
    }
    private IEnumerator RotateToAnglePID(float targetDegrees)
    {
        float targetAngle = GetCurrentAngle() + targetDegrees;
        float rotatedSoFar = -0.1f;
        float startAngle = GetCurrentAngle();
        float lastAngle = GetCurrentAngle();
       

        float lastError = 0f;

        while (true)
        {
            float currentAngle = GetCurrentAngle();

            // ✅ Always use DeltaAngle to handle wraparound correctly
            float delta = Mathf.DeltaAngle(lastAngle, currentAngle);
            rotatedSoFar += delta;
            lastAngle = currentAngle;

            float error = targetDegrees - rotatedSoFar;
            float derivative = (error - lastError) / Time.fixedDeltaTime;

            float output = kP * error + kD * derivative;

            JointMotor m = motor.motor;
            m.targetVelocity = Mathf.Clamp(output, -speed, speed);
            motor.motor = m;
            motor.useMotor = true;

            lastError = error;

            // Stop condition: we're close and slowing down
            if (Mathf.Abs(error) < 0.1f && Mathf.Abs(derivative) < 5f)
                break;

            yield return new WaitForFixedUpdate();
        }

        StopMotor();
        transform.localRotation = Quaternion.Euler(0, 0, startAngle + targetDegrees);
        Debug.Log("✅ PID Rotation complete.");
    }

    //private IEnumerator RotateToAngle(float targetDegrees)
    //{
    //    float rotatedSoFar = 0f;
    //    float lastAngle = GetCurrentAngle();
    //    JointMotor m = motor.motor;
    //    m.targetVelocity = Mathf.Sign(targetDegrees) * speed;
    //    m.freeSpin = false;
    //    motor.motor = m;
    //    motor.useMotor = true;

    //    while (Mathf.Abs(rotatedSoFar) + Mathf.Abs(AngleDelta(GetCurrentAngle(), lastAngle)) < Mathf.Abs(targetDegrees))

    //    {
    //        float currentAngle = GetCurrentAngle();
    //        float delta = AngleDelta(currentAngle, lastAngle);  // how much it changed this frame

    //        rotatedSoFar += delta;
    //        lastAngle = currentAngle;

    //        yield return null;
    //    }

    //    StopMotor();
    //    Debug.Log($"✅ Rotated total: {rotatedSoFar:F2} degrees.");
    //}


    // Get Y angle in local space
    private float GetCurrentAngle()
    {
        return motor.transform.localRotation.eulerAngles.z;
    }

    // Handles wrap-around (360° → 0°)
    private float AngleDelta(float a, float b)
    {
        return Mathf.DeltaAngle(b, a);
    }


    public Coroutine RunForSeconds(float seconds)
    {
        return StartCoroutine(RunMotorForDuration(seconds));
    }
    
    public void StopMotor()
    {
        JointMotor stopMotor = motor.motor;
        stopMotor.targetVelocity = 0f;
        motor.motor = stopMotor;
    }
    private IEnumerator RunMotorForDuration(float seconds)
    {
        JointMotor a = motor.motor;
        a.targetVelocity = speed;
        motor.motor = a;
        yield return new WaitForSeconds(seconds);
        StopMotor();
    }

    public void SetSpeed(float percent)
    {
        speed = MAX_SPEED * percent / 100;
    }

    public void SetPID(float newP, float newD)
    {
        kP = newP;
        kD = newD;
    }
}
