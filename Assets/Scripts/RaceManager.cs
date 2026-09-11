using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RaceManager : MonoBehaviour
{
    public Rigidbody m_PlayerCar;
    public CarCamera m_Camera;
    public GameUIManager m_UI;

    public float m_LoadInLength = 2.0f;
    public float m_CountdownLength = 3.0f;
    public float m_RaceEndLength = 5.0f;
    public float m_RespawnLength = 1.5f;

    public int m_MaxLaps;
    
    public RaceCheckpoint[] m_Checkpoints;

    public Vector3 m_RespawnOffset = new Vector3(0.0f, 2.0f, 0.0f);
    public Vector3 m_GizmoOffset = new Vector3(0.0f, 2.0f, 0.0f);

    private int m_CurrentLapsCompleted;

    public void Awake()
    {
        Application.targetFrameRate = 60;
    }
    public int CurrentLap
    {
        get { return m_CurrentLapsCompleted + 1; }
    }

    private int m_CurrentCheckpointIndex = -1;
    public int CurrentCheckpoint
    {
        get { return m_CurrentCheckpointIndex + 1; }
    }
    private int m_CurrentMajorCheckpointIndex = -1;

    private float m_CurrentLapTime;
    public float LapTime
    {
        get { return m_CurrentLapTime; }
    }
    private float m_CurrentRaceTime;
    public float RaceTime
    {
        get { return m_CurrentRaceTime; }
    }

    public enum RaceState
    {
        LOAD_IN,
        COUNTDOWN,
        IN_RACE,
        FINISHED
    }
    private RaceState m_CurrentState;
    public RaceState CurrentState
    {
        get { return m_CurrentState; }
    }
    private float m_StateTimer = 0.0f;
    public float StateTimer
    {
        get { return m_StateTimer; }
    }

    private bool m_NeedsRespawn = false;
    public bool Respawning
    {
        get { return m_NeedsRespawn; }
    }
    private float m_RespawnTimer = 0.0f;
    public float RespawnTimer
    {
        get { return m_RespawnTimer; }
    }

    private float m_FullTrackLength = 0.0f;
    public float FullTrackLength
    {
        get { return m_FullTrackLength; }
    }
    private float m_CompletedTrackLength = 0.0f;
    public float CompletedTrackLength
    {
        get { return m_CompletedTrackLength; }
    }

    private List<float> m_LapTimes = new List<float>();

    /// <summary>
    /// 
    /// </summary>
    public List<float> LapTimes
    {
        get
        {
            return m_LapTimes;
        }
    }

    /// <summary>
    /// Gizmo callback so that we can render appropriate scene gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (m_Checkpoints.Length > 0)
        {
            // Draw the total track
            Gizmos.color = Color.yellow;
            for (int i = 0; i < m_Checkpoints.Length; i++)
            {
                int next = (i + 1) % m_Checkpoints.Length;
                Gizmos.DrawLine(m_Checkpoints[i].transform.position + m_GizmoOffset,
                                m_Checkpoints[next].transform.position + m_GizmoOffset);
            }

            // Draw our "point on line"
            if (m_PlayerCar)
            {
                int safeIndex = m_CurrentCheckpointIndex + m_Checkpoints.Length;
                Vector3 currentCheckpoint = m_Checkpoints[(safeIndex) % m_Checkpoints.Length].transform.position;
                Vector3 nextCheckpoint = m_Checkpoints[(safeIndex + 1) % m_Checkpoints.Length].transform.position;
                float distance = GetClosestDistanceAlongLineSegment(currentCheckpoint, nextCheckpoint, m_PlayerCar.transform.position);
                Vector3 offset = (nextCheckpoint - currentCheckpoint).normalized * distance;
                    
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentCheckpoint + offset + m_GizmoOffset, 0.5f);
                Gizmos.DrawLine(m_PlayerCar.transform.position + m_GizmoOffset, currentCheckpoint + offset + m_GizmoOffset);
                Gizmos.DrawWireSphere(m_PlayerCar.transform.position + m_GizmoOffset, 0.5f);            
            }
        }
    }

    /// <summary>
    /// Helper function to rename all our checkpoint to be in order
    /// </summary>
    private void UpdateCheckpointNames()
    {
        for (int i = 0; i < m_Checkpoints.Length; i++)
        {
            if (m_Checkpoints[i] != null)
            {
                m_Checkpoints[i].name = $"Checkpoint_{i + 1}";
            }
        }
    }

    /// <summary>
    /// Editor-only function that is called when something changes, the scene starts, and 
    /// some other convenient locations/times
    /// </summary>
    private void OnValidate()
    {
        UpdateCheckpointNames();
    }

    /// <summary>
    /// Helper to check that we have finished the race
    /// </summary>
    /// <returns>If we have completed the race</returns>
    public bool HasFinishedRace()
    {
        return m_CurrentLapsCompleted >= m_MaxLaps;
    }

    /// <summary>
    /// Called when this gameObject is first made active
    /// </summary>
    private void Start()
    {
        m_PlayerCar.position = m_Checkpoints[m_Checkpoints.Length - 1].transform.position + m_RespawnOffset;
        m_PlayerCar.rotation = m_Checkpoints[m_Checkpoints.Length - 1].transform.rotation;

        TransitionToState(RaceState.LOAD_IN);
    }

    /// <summary>
    /// Start the car respawn (the car takes a small amount of time to reappear on the track)
    /// </summary>
    public void BeginCarRespawn()
    {
        m_UI.SpawnTextPopup("LOL RIP!");
        m_RespawnTimer = m_RespawnLength;
        m_Camera.m_FollowCar = false;
        m_NeedsRespawn = true;
    }

    /// <summary>
    /// Respawns the car at the furthest completed checkpoint
    /// </summary>
    public void RespawnCarAtCheckpoint()
    {
        // Select the appropriate checkpoint
        Vector3 targetPos;
        Quaternion targetRot;
        if (m_CurrentCheckpointIndex == -1)
        {
            targetPos = m_Checkpoints[m_Checkpoints.Length - 1].transform.position;
            targetRot = m_Checkpoints[m_Checkpoints.Length - 1].transform.rotation;        
        }
        else
        {
            targetPos = m_Checkpoints[m_CurrentCheckpointIndex].transform.position;
            targetRot = m_Checkpoints[m_CurrentCheckpointIndex].transform.rotation;
        }

        // Ensure we reset all body settings
        m_PlayerCar.transform.position = targetPos + m_RespawnOffset;
        m_PlayerCar.transform.rotation = targetRot;
        m_PlayerCar.linearVelocity = Vector3.zero;
        m_PlayerCar.angularVelocity = Vector3.zero;

        // Reset the camera as well
        m_Camera.m_FollowCar = true;
        m_Camera.ResetCamera();

        m_NeedsRespawn = false;
    }

    /// <summary>
    /// Used to ensure that we can only hit checkpoints that aren't skips
    /// </summary>
    /// <param name="_checkpoint">The checkpoint we wish to hit</param>
    public void AttemptToHitCheckpoint(RaceCheckpoint _checkpoint)
    {
        // Early exit if we've completed the race
        if (HasFinishedRace())
        {
            return;
        }

        // Iterate forward from current checkpoint
        // allow skipping through minor checkpoints
        // but major checkpoints must be hit
        for (int i = m_CurrentCheckpointIndex + 1; i < m_Checkpoints.Length; i++)
        {
            // checkpoint found
            if (_checkpoint == m_Checkpoints[i])
            {
                m_CurrentCheckpointIndex = i;
                _checkpoint.CompleteCheckpoint();
                if (_checkpoint.m_IsMajorCheckpoint)
                {
                    m_CurrentMajorCheckpointIndex = i;
                }
                if (i == m_Checkpoints.Length - 1)
                {
                    CompleteLap();
                }
                return;
            }

            // Made it to next major checkpoint without finding
            if (m_Checkpoints[i].m_IsMajorCheckpoint)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Called when we have completed a full lap of the course
    /// </summary>
    private void CompleteLap()
    {
        //store completed lap before reset/finish
        float completedLapTime = m_CurrentLapTime;
        //add complete lap to results list
        m_LapTimes.Add(completedLapTime);

        // Save best lap time if better
        float oldBestLapTime = PlayerPrefs.GetFloat($"BestLapTime_{SceneManager.GetActiveScene().name}", 99999.0f);
        bool newBestLap = completedLapTime < oldBestLapTime;
        if (newBestLap)
        {
            PlayerPrefs.SetFloat($"BestLapTime_{SceneManager.GetActiveScene().name}", completedLapTime);
        }

        // Reset all checkpoints
        for (int i = m_CurrentCheckpointIndex; i < m_Checkpoints.Length; i++)
        {
            m_Checkpoints[i].ResetCheckpoint();
        }
        m_CurrentCheckpointIndex = -1;
        m_CurrentMajorCheckpointIndex = -1;

        // Increment lap
        m_CurrentLapsCompleted++;
        //reset timer for next lap
        m_CurrentLapTime = 0.0f;

        //finish race or lap complete popup
        if (HasFinishedRace())
        {
            CompleteRace();
        }
        else
        {
            m_UI.ShowLapComplete(completedLapTime, newBestLap);
        }
    }

    /// <summary>
    /// Called when we have completed the full race (normally 3 laps)
    /// </summary>
    private void CompleteRace()
    {
        // Transition to "finished"
        TransitionToState(RaceState.FINISHED);

        string bestRaceKey = $"BestRaceTime_{SceneManager.GetActiveScene().name}";
        bool hadPreviousBest = PlayerPrefs.HasKey(bestRaceKey);
        float oldBestRaceTime = PlayerPrefs.GetFloat($"BestRaceTime_{SceneManager.GetActiveScene().name}", 99999.0f);
        bool newBestRace = m_CurrentRaceTime < oldBestRaceTime;

        if (newBestRace)
        {
            PlayerPrefs.SetFloat(bestRaceKey, m_CurrentRaceTime);    
        }
        m_UI.ShowRaceResults(oldBestRaceTime, newBestRace, hadPreviousBest);
    }

    /// <summary>
    /// Handles all our state transitions
    /// </summary>
    /// <param name="_state">The transition to move to</param>
    private void TransitionToState(RaceState _state)
    {
        m_CurrentState = _state;
        switch (_state)
        {
            case RaceState.LOAD_IN: // This is the "starting period" where we sit and see the track
                {
                    //m_UI.SpawnTextPopup("Course Name");
                    m_StateTimer = m_LoadInLength;
                    // Pauses car and camera
                    m_PlayerCar.GetComponent<CarControl>().Controllable = false;
                    m_Camera.m_FollowCar = false;
                    m_Camera.m_LookAtCar = false;
                } break;
            case RaceState.COUNTDOWN: // The classic race 3, 2, 1 countdown
                {
                    //m_UI.SpawnTextPopup("Countdown!");
                    m_StateTimer = m_CountdownLength;
                    // Brings camera in naturally
                    m_Camera.m_FollowCar = true;
                } break;
            case RaceState.IN_RACE: // We are racing!
                {
                    //m_UI.SpawnTextPopup("Race Begun!");
                    m_StateTimer = -1.0f;
                    // Camera now looking properly and car controllable
                    m_PlayerCar.GetComponent<CarControl>().Controllable = true;
                    m_Camera.m_LookAtCar = true;
                } break;
            case RaceState.FINISHED: // The race is over, delay before loading to menu
                {
                    //m_UI.SpawnTextPopup("Race Complete!");
                    m_StateTimer = m_RaceEndLength;
                    // Let car roll and hold camera to get a cool pan shot
                    m_PlayerCar.GetComponent<CarControl>().Controllable = false;
                    m_Camera.m_FollowCar = false;
                } break;
            default: {} break;
        }
    }

    private Vector3 GetClosestPointOnLine(Vector3 _a, Vector3 _b, Vector3 _p)
    {
        Vector3 pointOffset = _p - _a;
        Vector3 direction = (_b - _a).normalized;

        float distance = Vector3.Distance(_a, _b);
        float alpha = Vector3.Dot(direction, pointOffset);

        if (alpha <= 0.0f) // behind first point
        {
            return _a;
        }
        else if (alpha >= distance) // past second point
        {
            return _b;
        }

        return _a + (direction * alpha);
    }

    /// <summary>
    /// Used to calculate how far along our line segment we are (by closest point to line)
    /// </summary>
    /// <param name="_a">Point A on the line</param>
    /// <param name="_b">Point B on the line</param>
    /// <param name="_p">Our position (may or may not be on the line)</param>
    /// <returns></returns>
    private float GetClosestDistanceAlongLineSegment(Vector3 _a, Vector3 _b, Vector3 _p)
    {
        Vector3 pointOffset = _p - _a;
        Vector3 direction = (_b - _a).normalized;

        float distance = Vector3.Distance(_a, _b);
        float alpha = Vector3.Dot(direction, pointOffset);

        alpha = Mathf.Clamp(alpha, 0.0f, distance);
        return alpha;
    }

    /// <summary>
    /// Calculate how far along the full track we are
    /// </summary>
    private void CalculateTrackPercentage()
    {
        m_FullTrackLength = 0.0f;
        m_CompletedTrackLength = 0.0f;
        for (int i = 0; i < m_Checkpoints.Length; i++)
        {
            Vector3 current = m_Checkpoints[(i + m_Checkpoints.Length - 1) % m_Checkpoints.Length].transform.position;
            Vector3 next = m_Checkpoints[i].transform.position;
            float distance = (current - next).magnitude;
            m_FullTrackLength += distance;
            if (i < m_CurrentCheckpointIndex + 1)
            {
                m_CompletedTrackLength += distance;
            }
            else if (i == m_CurrentCheckpointIndex + 1)
            {
                m_CompletedTrackLength += GetClosestDistanceAlongLineSegment(current, next, m_PlayerCar.transform.position);
            }
        }
    }

    /// <summary>
    /// Our update loop from Unity, called as fast as possible 
    /// </summary>
    private void Update()
    {
        // Progress our car respawn if appropriate
        if (m_NeedsRespawn)
        {
            m_RespawnTimer -= Time.deltaTime;
            if (m_RespawnTimer <= 0.0f)
            {
                RespawnCarAtCheckpoint();
            }
        }

        // Updates track percentage 
        CalculateTrackPercentage();

        // State-based updates
        switch (m_CurrentState)
        {
            case RaceState.LOAD_IN:
                {
                    m_StateTimer -= Time.deltaTime;
                    if (m_StateTimer <= 0.0f)
                    {
                        TransitionToState(RaceState.COUNTDOWN); // Move to next state
                    }
                } break;
            case RaceState.COUNTDOWN:
                {
                    m_StateTimer -= Time.deltaTime;
                    if (m_StateTimer <= 0.0f)
                    {
                        TransitionToState(RaceState.IN_RACE); // Move to next state
                    }
                } break;
            case RaceState.IN_RACE:
                {
                    // Just increment timers while in race
                    m_CurrentLapTime += Time.deltaTime;
                    m_CurrentRaceTime += Time.deltaTime;
                } break;
            case RaceState.FINISHED:
                {
                    //stay on results screen until player picks an option
                } break;
            default: {} break;
        } 
    }
}
