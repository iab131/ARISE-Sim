using System.Collections;
using UnityEngine;

public class DrivetrainController : MonoBehaviour
{
    private SimMotor leftMotor;
    private SimMotor rightMotor;
    private float speedPercent = 100f;
    public static DrivetrainController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    public void SetSpeedPercent(float percent)
    {
        speedPercent = percent;
        if (NullCheck()) return;
        leftMotor.SetSpeed(percent);
        rightMotor.SetSpeed(percent);
    }

    public void AssignMotorsByLabel(char leftLabel, char rightLabel)
    {
        leftMotor = MotorSimulationManager.Instance.GetMotor(leftLabel);
        rightMotor = MotorSimulationManager.Instance.GetMotor(rightLabel);
        //leftMotor.SetPID(8, 0.8f);
        //rightMotor.SetPID(8, 0.8f);
        SetSpeedPercent(speedPercent);
    }


    public IEnumerator DriveForDegrees(float degrees)
    {
        if (NullCheck()) yield break;
     
        Coroutine leftCoroutine = StartCoroutine(leftMotor.RotateByDegrees(degrees));
        Coroutine rightCoroutine = StartCoroutine(rightMotor.RotateByDegrees(-degrees));

        yield return new WaitUntil(() => !leftMotor.isRotating && !rightMotor.isRotating);
        Stop();
    }


    public IEnumerator DriveForSeconds(float seconds, bool forward) {
        if (NullCheck()) yield break;

        Coroutine leftCoroutine = StartCoroutine(leftMotor.RunForSeconds(seconds, forward));
        Coroutine rightCoroutine = StartCoroutine(rightMotor.RunForSeconds(seconds, !forward));

        yield return new WaitUntil(() => !leftMotor.isRotating && !rightMotor.isRotating);
        Stop();
    }

    public void StartDriving(bool forward)
    {
        if (NullCheck()) return;
        leftMotor.RotateForever(forward);
        rightMotor.RotateForever(!forward);
    }

    public IEnumerator TurnForDegrees(float steer, float degrees)
    {
        if (NullCheck()) yield break;
        // negative turnspeed means left

        float[] speeds = CalcTurnSpeed(steer);  // steer right 50
        float leftSpeed = speeds[0];
        float rightSpeed = speeds[1];

        leftMotor.SetSpeed(leftSpeed);
        rightMotor.SetSpeed(rightSpeed);
        Coroutine leftCoroutine = StartCoroutine(leftMotor.RotateByDegrees(degrees));
        Coroutine rightCoroutine = StartCoroutine(rightMotor.RotateByDegrees(-degrees));

        yield return new WaitUntil(() => !leftMotor.isRotating && !rightMotor.isRotating);
        Stop();
    }
    public IEnumerator TurnForSeconds(float steer, float seconds)
    {
        if (NullCheck()) yield break;

        float[] speeds = CalcTurnSpeed(steer);  // steer right 50
        float leftSpeed = speeds[0];
        float rightSpeed = speeds[1];

        leftMotor.SetSpeed(leftSpeed);
        rightMotor.SetSpeed(rightSpeed);
        
        Coroutine leftCoroutine = StartCoroutine(leftMotor.RunForSeconds(seconds, true));
        Coroutine rightCoroutine = StartCoroutine(rightMotor.RunForSeconds(seconds, false));

        yield return new WaitUntil(() => !leftMotor.isRotating && !rightMotor.isRotating);
        Stop();
    }
    public void StartTurning(float steer)
    {
        if (NullCheck()) return;
        float[] speeds = CalcTurnSpeed(steer);  // steer right 50
        float leftSpeed = speeds[0];
        float rightSpeed = speeds[1];

        leftMotor.SetSpeed(leftSpeed);
        rightMotor.SetSpeed(rightSpeed);

        leftMotor.RotateForever(true);
        rightMotor.RotateForever(false);
    }

    public void Stop()
    {
        if (NullCheck()) return;
        SetSpeedPercent(speedPercent);
        leftMotor.StopMotor();
        rightMotor.StopMotor();
    }
    private float[] CalcTurnSpeed(float steer)
    {
        float left = speedPercent;
        float right = speedPercent;

        if (steer > 0f)                       // turn right
        {
            // fade / reverse the right wheel
            right = speedPercent * (1f - 2f * steer / 100f);
            //rightMotor.SetForce(right);
        }
        else if (steer < 0f)                  // turn left
        {
            // fade / reverse the left wheel
            left = speedPercent * (1f + 2f * steer / 100f);
            //leftMotor.SetForce(left);
        }

        return new float[] { left, right };
    }
    public bool NullCheck()
    {
        return leftMotor == null || rightMotor == null;
    }
}
