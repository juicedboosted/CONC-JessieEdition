using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles steering input from screen analog stick
/// Initial touch begins inside stick area
/// </summary>
public class AnalogStickInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI")]
    [SerializeField] private RectTransform m_background;
    [SerializeField] private RectTransform m_handle;

    [Header("SETTINGS")]
    [SerializeField] private float m_handleRange = 100.0f;

    private Vector2 m_input;
    private bool m_isActive = false;

    //returns horizontal position of stick as steering input
    public float SteeringInput => m_input.x;

    /// <summary>
    /// Starts input if initial touch is inside the valid area
    /// </summary>
    /// <param name="_eventData"></param>
    public void OnPointerDown(PointerEventData _eventData)
    {
        Vector2 localPoint;
        //convert screen touch position to sticks local coords
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_background, _eventData.position, _eventData.pressEventCamera, out localPoint))
        {
            //ignore touch that start outside analog circle
            if (localPoint.magnitude <= m_handleRange)
            {
                m_isActive = true;
                UpdateStick(localPoint);
            }
        }
    }

    /// <summary>
    /// Updates analog stick position while player has an active touch
    /// </summary>
    /// <param name="_eventData"></param>
    public void OnDrag(PointerEventData _eventData)
    {
        if (!m_isActive)
        {
            return;
        }
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_background, _eventData.position, _eventData.pressEventCamera, out localPoint))
        {
            UpdateStick(localPoint);
        }
    }

    /// <summary>
    /// Ends analog stick input and returns handle + steering value to its neutral position
    /// </summary>
    /// <param name="_eventData"></param>
    public void OnPointerUp(PointerEventData _eventData)
    {
        m_isActive = false;
        m_input = Vector2.zero;
        m_handle.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// Moves handle within allowed range and converts position into normalised input
    /// </summary>
    /// <param name="_localPoint"></param>
    private void UpdateStick(Vector2 _localPoint)
    {
        Vector2 clampedPosition = Vector2.ClampMagnitude(_localPoint, m_handleRange);
        m_handle.anchoredPosition = clampedPosition;
        //convert position into input range (-1 to 1)
        m_input = clampedPosition / m_handleRange;
        m_input = Vector2.ClampMagnitude(m_input, 1.0f);
    }
}
