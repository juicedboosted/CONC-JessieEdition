using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
///  Manages mobile control settings
///  Steering mode, Steering sensitivity, Tilt Calibration, saved preferences
/// </summary>
public class MobileSettingsManager : MonoBehaviour
{
    /// <summary>
    /// Steering options
    /// </summary>
    public enum InputMode
    {
        AnalogStick,
        RelativeTouch,
        Rotation
    }

    [Header("UI")]
    [SerializeField] private TMP_Text m_inputModeText;
    [SerializeField] private Slider m_sensitivitySlider;
    [SerializeField] private TMP_Text m_sensitivityValueText;
    [SerializeField] private GameObject m_calibrateButton;

    [Header("Defaults")]
    [SerializeField] private float m_defaultSensitivity = 1.0f;


    private InputMode m_currentInputMode;
    public InputMode CurrentInputMode => m_currentInputMode;
    public float Sensitivity => m_sensitivitySlider.value;

    /// <summary>
    /// Loads previous settings
    /// </summary>
    private void Start()
    {
        LoadSettings();

        //listener for changes on sensitivity slider
        m_sensitivitySlider.onValueChanged.AddListener(OnSensitivityChange);
        UpdateUI();
    }

    /// <summary>
    /// Loads saved settings from PlayerPrefs, otherwise uses default values
    /// </summary>
    private void LoadSettings()
    {
        int savedMode = PlayerPrefs.GetInt("InputMode", 0);
        m_currentInputMode = (InputMode)savedMode;
        float savedSensitivity = PlayerPrefs.GetFloat("SteeringSensitivity", m_defaultSensitivity);
        m_sensitivitySlider.value = savedSensitivity;
    }

    /// <summary>
    /// Cycle to next steering input
    /// </summary>
    public void NextInputMode()
    {
        int mode = (int)m_currentInputMode + 1;
        if (mode > 2)
        {
            mode = 0;
        }
        SetInputMode((InputMode)mode);
    }

    /// <summary>
    /// Cycle to previous steering input
    /// </summary>
    public void PreviousInputMode()
    {
        int mode = (int)m_currentInputMode - 1;
        if (mode < 0)
        {
            mode = 2;
        }
        SetInputMode((InputMode)mode);
    }

    /// <summary>
    /// Change and save selected input
    /// </summary>
    public void SetInputMode(InputMode _inputMode)
    {
        m_currentInputMode = _inputMode;
        
        //save selected mode so it persists
        PlayerPrefs.SetInt("InputMode", (int)m_currentInputMode);
        PlayerPrefs.Save();
        UpdateUI();

        MobileInputManager inputManager = FindFirstObjectByType<MobileInputManager>();
        if (inputManager != null)
        {
            inputManager.RefreshInputMode();
        }
    }

    /// <summary>
    /// Saves steering sensitivity when slider changes
    /// </summary>
    private void OnSensitivityChange(float _value)
    {
        PlayerPrefs.SetFloat("SteeringSensitivity", _value);
        PlayerPrefs.Save();
        UpdateSensitivityText();
    }

    /// <summary>
    /// Updates all settings menu UI
    /// </summary>
    private void UpdateUI()
    {
        UpdateInputModeText();
        UpdateSensitivityText();

        //calibration for tilt steering (only when using device tilt)
        m_calibrateButton.SetActive(m_currentInputMode == InputMode.Rotation);
    }

    /// <summary>
    /// Updates displayed steering mode
    /// </summary>
    private void UpdateInputModeText()
    {
        switch (m_currentInputMode)
        {
            case InputMode.AnalogStick:
                m_inputModeText.text = "ANALOG STICK";
                break;
            case InputMode.RelativeTouch:
                m_inputModeText.text = "RELATIVE TOUCH";
                break;
            case InputMode.Rotation:
                m_inputModeText.text = "DEVICE TILT";
                break;
        }
    }

    /// <summary>
    /// Updates displayed sensitivity value
    /// </summary>
    private void UpdateSensitivityText()
    {
        m_sensitivityValueText.text = m_sensitivitySlider.value.ToString("0.0");
    }

    /// <summary>
    /// Recalibration of tilt steering so current device angle is the neutrol position
    /// </summary>
    public void CalibrateTilt()
    {
        RotationInput rotationInput = FindFirstObjectByType<RotationInput>();

        if (rotationInput != null)
        {
            rotationInput.Calibrate();
        }
    }
}
