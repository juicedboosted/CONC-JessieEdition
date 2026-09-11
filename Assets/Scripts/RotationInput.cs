using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Handles steering using device rotation.
/// Calibration, deadzone, smoothing
/// </summary>
public class RotationInput : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float m_maxTiltAngle = 25.0f;
    [SerializeField] private float m_deadZoneAngle = 2.0f;
    [SerializeField] private float m_smoothingSpeed = 5.0f;

    private float m_calibratedAngle = 0.0f;
    private float m_steeringInput = 0.0f;

    //returns normalised tilt steering value
    public float SteeringInput => m_steeringInput;

    /// <summary>
    /// Sets device current rotation as neutral steering position
    /// </summary>
    public void Calibrate()
    {
        if (AttitudeSensor.current == null)
        {
            return;
        }
        Quaternion attitude = AttitudeSensor.current.attitude.ReadValue();
        m_calibratedAngle = NormalizeAngle(attitude.eulerAngles.z);
        m_steeringInput = 0.0f;
    }

    /// <summary>
    /// Converts angle from 0 to 360 to signed -180 to 180
    /// </summary>
    /// <param name="_angle"></param>
    /// <returns></returns>
    private float NormalizeAngle(float _angle)
    {
        if (_angle > 180.0f)
        {
            _angle -= 360.0f;
        }
        return _angle;
    }


    /// <summary>
    /// Enables device rotation sensor and calibrates to initial device position
    /// </summary>
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
        }
        Calibrate();
    }

    /// <summary>
    /// Uses current device rotation to convert tilt to normalised steering value
    /// </summary>
    // Update is called once per frame
    void Update()
    {
        if (AttitudeSensor.current  == null)
        {
            m_steeringInput = 0.0f;
            return;
        }
        Quaternion attitude = AttitudeSensor.current.attitude.ReadValue();
        float currentAngle = NormalizeAngle(attitude.eulerAngles.z);
        float difference = Mathf.DeltaAngle(m_calibratedAngle, currentAngle);
        float targetInput = 0.0f;

        //ignore small device movements
        if (Mathf.Abs(difference) > m_deadZoneAngle)
        {
            float adjustedDifference = difference - Mathf.Sign(difference) * m_deadZoneAngle;
            float usableAngle = m_maxTiltAngle - m_deadZoneAngle;
            targetInput = Mathf.Clamp(-adjustedDifference / usableAngle, -1.0f, 1.0f);
        }
        m_steeringInput = Mathf.MoveTowards(m_steeringInput, targetInput, m_smoothingSpeed * Time.deltaTime);
    }
}
