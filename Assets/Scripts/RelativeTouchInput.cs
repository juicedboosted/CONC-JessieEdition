using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handle relative touch steering input
/// Steering is calculated from horizontal distance between initial touch position
/// and current touch position
/// </summary>
public class RelativeTouchInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Settings")]
    [SerializeField] private float m_maxDragDistance = 300.0f;
    [SerializeField] private float m_deadZone = 20.0f;

    private Vector2 m_startPosition;
    private float m_steeringInput = 0.0f;
    private bool m_isActive = false;
    
    //normalised steering value (-1 and 1)
    public float SteeringInput => m_steeringInput;

    /// <summary>
    /// Start relative steering and save initial touch position
    /// </summary>
    /// <param name="_eventData"></param>
    public void OnPointerDown(PointerEventData _eventData)
    {
        m_startPosition = _eventData.position;
        m_steeringInput = 0.0f;
        m_isActive = true;
    }

    /// <summary>
    /// Updates steering based on horizontal distance travelled from initial touch
    /// </summary>
    /// <param name="_eventData"></param>
    public void OnDrag(PointerEventData _eventData)
    {
        if (!m_isActive)
        {
            return;
        }
        float horizontalDifference = _eventData.position.x - m_startPosition.x;

        //ignore small movements
        if (Mathf.Abs(horizontalDifference) < m_deadZone)
        {
            m_steeringInput = 0.0f;
            return;
        }

        float adjustedDifference = horizontalDifference - Mathf.Sign(horizontalDifference) * m_deadZone;
        float usableDistance = m_maxDragDistance - m_deadZone;

        //drag distance -> normalised steering value
        m_steeringInput = adjustedDifference / usableDistance;
        m_steeringInput = Mathf.Clamp(m_steeringInput, -1.0f, 1.0f);
    }

    /// <summary>
    /// Stops relative steering and resets input
    /// </summary>
    /// <param name="_eventData"></param>
    public void OnPointerUp(PointerEventData _eventData)
    {
        m_isActive = false;
        m_steeringInput = 0.0f;
    }
}
