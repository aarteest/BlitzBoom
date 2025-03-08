using UnityEngine;

public class PIDController
{
    public float Kp, Ki, Kd; // PID constants
    private float previousError;
    private float integral;

    public PIDController(float kp, float ki, float kd)
    {
        Kp = kp;
        Ki = ki;
        Kd = kd;
    }

    public float Compute(float error, float deltaTime)
    {
        integral += error * deltaTime;
        float derivative = (error - previousError) / deltaTime;
        previousError = error;

        return (Kp * error) + (Ki * integral) + (Kd * derivative);
    }
}
