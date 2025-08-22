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
    
    public float speed;
    private const float MAX_SPEED = 800f;
    private const float FORCE = 100f;
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

        speed = MAX_SPEED;
    }
    public bool isRotating { get; private set; }

    private void OnDestroy()
    {
        allMotors.Remove(this);
        MotorSimulationManager.Instance.ClearMotor(this);
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

    public IEnumerator RotateRotations(float rotations)
    {
        yield return RotateByDegrees(rotations * 360f);
    }

    public IEnumerator RotateByDegrees(float degrees)
    {
        yield return RotateToAnglePID(degrees);
    }
    private IEnumerator RotateToAnglePID(float targetDegrees)
    {
        isRotating = true;
        if (speed < 0)
        {
            targetDegrees = -targetDegrees;
        }
        float absSpeed = Mathf.Abs(speed);

        float rotatedSoFar = 0f;
        float lastAngle = GetCurrentAngle();   // 1st frame reference
        float lastError = targetDegrees;       // full error at start

        // ––– settle logic –––
        float settleTimer = 0f;
        const float settleTimeNeeded = 0.20f;       // seconds inside tolerance
        const float errorTol = 0.5f;                // deg
        const float derivTol = 5f;                  // deg / s

        while (true)
        {
            float current = GetCurrentAngle();

            // signed angular change this physics step (wrap-safe)
            float delta = Mathf.DeltaAngle(lastAngle, current);
            rotatedSoFar += delta;
            lastAngle = current;

            float error = targetDegrees - rotatedSoFar;
            float derivative = (error - lastError) / Time.fixedDeltaTime;
            lastError = error;

            // PID output → targetVelocity (clamped to ±speed)
            float output = kP * error + kD * derivative;
            JointMotor jm = motor.motor;
            jm.targetVelocity = Mathf.Clamp(output, -absSpeed, absSpeed);
            jm.force = FORCE;          // still positive!
            motor.motor = jm;
            motor.useMotor = true;

            /* ---------- stop condition ---------- */
            if (Mathf.Abs(error) < errorTol && Mathf.Abs(derivative) < derivTol)
            {
                settleTimer += Time.fixedDeltaTime;
                if (settleTimer >= settleTimeNeeded) break;   // ✅ done
            }
            else
            {
                settleTimer = 0f;   // not stable yet
            }
            /* ------------------------------------ */

            yield return new WaitForFixedUpdate();
        }

        StopMotor();        // sets targetVelocity = 0, isRotating = false
        Debug.Log($"✅ PID complete on [{motorLabel}]  rotated {rotatedSoFar:F1}°");
    }



    // Get Y angle in local space
    private float GetCurrentAngle()
    {
        return motor.transform.localRotation.eulerAngles.z;
    }

    public IEnumerator RunForSeconds(float seconds, bool forward)
    {
        isRotating = true;
        JointMotor a = motor.motor;
        int sign = forward ? 1 : -1;
        a.targetVelocity = speed * sign;
        a.force = FORCE;
        motor.motor = a;
        yield return new WaitForSeconds(seconds);
        StopMotor();
    }
    
    public void RotateForever(bool forward)
    {
        isRotating = true;
        JointMotor a = motor.motor;
        int sign = forward ? 1 : -1;
        a.force = FORCE;
        a.targetVelocity = speed * sign;
        motor.motor = a;
    }
    public void StopMotor()
    {
        isRotating = false;
        JointMotor stopMotor = motor.motor;
        stopMotor.targetVelocity = 0f;
        stopMotor.force = FORCE;
        motor.motor = stopMotor;
    }

    public void SetSpeed(float percent)
    {
        percent = Mathf.Clamp(percent,-100,100);
        speed = MAX_SPEED * percent / 100;
        //Debug.Log(speed);
    }

    //public void SetForce(float percent)
    //{
    //    percent = Mathf.Abs(percent);
    //    JointMotor m = motor.motor;
    //    m.force = percent * FORCE / 100; 
    //    motor.motor = m;
    //    Debug.Log("new force"+m.force);
    //}

    public void SetPID(float newP, float newD)
    {
        kP = newP;
        kD = newD;
    }
}
