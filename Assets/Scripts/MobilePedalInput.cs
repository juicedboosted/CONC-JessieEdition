using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles acceleration and brake pedal input through the touchscreen
/// Returns normalised driving value while pedal is being held
/// </summary>
public class MobilePedalInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// Available pedals for driving controls
    /// </summary>
    public enum PedalType
    {
        Accelerate,
        Brake
    }

    [SerializeField] private PedalType m_pedalType;

    private bool m_isPressed = false;
    
    /// <summary>
    /// Returns pedal input
    /// Accelerate -> 1
    /// Brake -> -1
    /// Unpressed -> 0
    /// </summary>
    public float InputValue
    {
        get
        {
            if (!m_isPressed)
            {
                return 0.0f;
            }
            return m_pedalType == PedalType.Accelerate ? 1.0f : -1.0f;
        }
    }

    /// <summary>
    /// Activates pedal when player presses the button
    /// </summary>
    /// <param name="_eventData"></param>
    public void OnPointerDown(PointerEventData _eventData)
    {
        m_isPressed = true;
    }

    /// <summary>
    /// Releases pedal when player stops pressing the button
    /// </summary>
    /// <param name="_eventData"></param>
    public void OnPointerUp(PointerEventData _eventData)
    {
        m_isPressed = false;
    }
}
