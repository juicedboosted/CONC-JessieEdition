using UnityEngine;

/// <summary>
/// Mobile driving input manager
/// Selects active steering mode and applies steering sensitivity
/// Manages control visbility and provides steering/pedal input to car
/// </summary>
public class MobileInputManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private AnalogStickInput m_analogStick;
    [SerializeField] private RelativeTouchInput m_relativeTouch;
    [SerializeField] private RotationInput m_rotationInput;
    [SerializeField] private MobilePedalInput m_acceleratePedal;
    [SerializeField] private MobilePedalInput m_brakePedal;


    [Header("Settings")]
    [SerializeField] private float m_defaultSensitivity = 1.0f;

    private MobileSettingsManager.InputMode m_currentInputMode;

    /// <summary>
    /// returns a normalised steer value from selected steering mode
    /// </summary>
    public float SteeringInput
    {
        get
        {
            float input = 0.0f;
            switch (m_currentInputMode)
            {
                case MobileSettingsManager.InputMode.AnalogStick:
                    if(m_analogStick != null)
                    {
                        input = m_analogStick.SteeringInput;

                        //stop small accidental steering
                        if (Mathf.Abs(input) < 0.1f)
                        {
                            input = 0.0f;
                        }
                    }
                    break;
                case MobileSettingsManager.InputMode.RelativeTouch:
                    if (m_relativeTouch != null)
                    {
                        input = m_relativeTouch.SteeringInput;
                    }
                    break;
                case MobileSettingsManager.InputMode.Rotation:
                    if (m_rotationInput != null)
                    {
                        input = m_rotationInput.SteeringInput;
                    }
                    break;
            }
            //saved steering sensitivity
            float sensitivity = PlayerPrefs.GetFloat("SteeringSensitivity", m_defaultSensitivity);
            input *= sensitivity;

            return Mathf.Clamp(input, -1.0f, 1.0f);
        }
    }

    /// <summary>
    /// Enables only the steering required by current input mode
    /// </summary>
    public void UpdateControlVisibility()
    {
        if (m_analogStick != null)
        {
            m_analogStick.gameObject.SetActive(m_currentInputMode == MobileSettingsManager.InputMode.AnalogStick);
        }
        if (m_relativeTouch != null)
        {
            m_relativeTouch.gameObject.SetActive(m_currentInputMode == MobileSettingsManager.InputMode.RelativeTouch);
        }
        if (m_rotationInput != null)
        {
            m_rotationInput.enabled = m_currentInputMode == MobileSettingsManager.InputMode.Rotation;
        }
    }

    /// <summary>
    /// Reloads saved steering mode and updates what controls are active
    /// </summary>
    public void RefreshInputMode()
    {
        m_currentInputMode = (MobileSettingsManager.InputMode)PlayerPrefs.GetInt("InputMode", 0);
        UpdateControlVisibility();
    }

    /// <summary>
    /// returns combined acceleration and brake input
    /// positive -> accelerate
    /// negative -> brake/reverse
    /// </summary>
    public float DriveInput
    {
        get
        {
            float accelerate = 0.0f;
            float brake = 0.0f;

            if (m_acceleratePedal != null)
            {
                accelerate = m_acceleratePedal.InputValue;
            }
            if (m_brakePedal != null)
            {
                brake = m_brakePedal.InputValue;
            }
            return Mathf.Clamp(accelerate + brake, -1.0f, 1.0f);
        }
    }

    /// <summary>
    /// Finds input not manually assigned and loads saved steering mode
    /// </summary>
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_currentInputMode = (MobileSettingsManager.InputMode)PlayerPrefs.GetInt("InputMode", 0);
        if (m_analogStick == null)
        {
            m_analogStick = FindFirstObjectByType<AnalogStickInput>();
        }
        if (m_relativeTouch == null)
        {
            m_relativeTouch = FindFirstObjectByType<RelativeTouchInput>(FindObjectsInactive.Include);
        }
        if (m_rotationInput == null)
        {
            m_rotationInput = FindFirstObjectByType<RotationInput>();
        }
        UpdateControlVisibility();
    }
}
