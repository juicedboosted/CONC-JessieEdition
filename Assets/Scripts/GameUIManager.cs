using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manages gameplay user interface, HUD, race info, countdowns, lap/race results, pause menu,
/// settings menu, UI feedback
/// Updates race data and also controls transistions between menus
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("External Components")]
    public RaceManager m_RaceManager;
    public Rigidbody m_PlayerCar;

    [Header("Internal Components")]
    public TextMeshProUGUI m_TextDisplay;

    [Header("Popup Settings")]
    public GameObject m_TextPopup;
    public float m_PopupLifeLength = 0.9f;

    [Header("Speedometer")]
    [SerializeField] private Image[] m_speedBars;
    [SerializeField] private Color m_speedActiveColor = new Color(1f, 0.55f, 0f);
    [SerializeField] private Color m_speedInactiveColor = Color.grey;

    [Header("HUD Components")]
    public TextMeshProUGUI m_LapInfo;
    public TextMeshProUGUI m_TimeInfo;
    public TextMeshProUGUI m_ProgressInfo;
    public TextMeshProUGUI m_Speedometer;
    public TextMeshProUGUI m_BestLapInfo;
    public TextMeshProUGUI m_CurrentLapInfo;
    public TextMeshProUGUI m_CountDownText;
    public TextMeshProUGUI m_LapCompleteText;
    [SerializeField] private GameObject m_BestLapStar;

    [SerializeField] private GameObject m_PauseMenu;
    [SerializeField] private GameObject m_gameSettingsMenu;
    private bool m_isPaused = false;


    [Header("Results Screen")]
    public GameObject m_ResultsScreen;
    public GameObject m_GameplayHUD;

    public TextMeshProUGUI m_TotalRaceText;
    public TextMeshProUGUI m_Lap1Text;
    public TextMeshProUGUI m_Lap2Text;
    public TextMeshProUGUI m_Lap3Text;
    public TextMeshProUGUI m_PersonalBestText;
    [SerializeField] private Color m_bestLapColor = new Color(1f, 0.55f, 0f);

    private RaceManager.RaceState m_previousState;

    [Header("Input")]
    public InputActionAsset m_InputActions;

    /// <summary>
    /// Function to resume time and deactivate menus
    /// </summary>
    public void ResumeGame()
    {
        m_isPaused = false;
        m_PauseMenu.SetActive(false);
        m_gameSettingsMenu.SetActive(false);
        Time.timeScale = 1.0f;
    }
    /// <summary>
    /// Function to resume time and reload current scene
    /// </summary>
    public void RetryRace()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Function to resume time and go to the main menu scene
    /// </summary>
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Display final race results with lap times, total race time and highlight fastest lap and personal best 
    /// </summary>
    public void ShowRaceResults(float _oldBestRaceTime, bool _newBestRaceTime, bool _hadPreviousBest)
    {
        m_GameplayHUD.SetActive(false);
        m_ResultsScreen.SetActive(true);
        int fastestLapIndex = 0;
        for (int i = 0; i < m_RaceManager.LapTimes.Count; i++)
        {
            if (m_RaceManager.LapTimes[i] < m_RaceManager.LapTimes[fastestLapIndex])
            {
                fastestLapIndex = i;
            }
        }
        //reset text colors
        m_Lap1Text.color = Color.white;
        m_Lap2Text.color = Color.white;
        m_Lap3Text.color = Color.white;

        if (fastestLapIndex == 0)
        {
            m_Lap1Text.color = m_bestLapColor;
        }
        else if (fastestLapIndex == 1)
        {
            m_Lap2Text.color = m_bestLapColor;
        }
        else if (fastestLapIndex == 2)
        {
            m_Lap3Text.color = m_bestLapColor;
        }

        m_ResultsScreen.SetActive(true);
        //total race time
        float raceTime = m_RaceManager.RaceTime;

        int raceMinutes = Mathf.FloorToInt(raceTime / 60);
        int raceSeconds = Mathf.FloorToInt(raceTime % 60);
        int raceHundredths = Mathf.FloorToInt((raceTime * 100f) % 100f);

        m_TotalRaceText.text = $"TOTAL TIME:\n{raceMinutes:00}:{raceSeconds:00}:{raceHundredths:00}";

        //lap 1
        if (m_RaceManager.LapTimes.Count > 0)
        {
            float lap1 = m_RaceManager.LapTimes[0];
            int minutes = Mathf.FloorToInt(lap1 / 60f);
            int seconds = Mathf.FloorToInt(lap1 % 60f);
            int hundredths = Mathf.FloorToInt((lap1 * 100f) % 100f);

            m_Lap1Text.text = $"LAP 1\n{minutes:00}:{seconds:00}:{hundredths:00}";
        }
        //lap 2
        if (m_RaceManager.LapTimes.Count > 1)
        {
            float lap2 = m_RaceManager.LapTimes[1];
            int minutes = Mathf.FloorToInt(lap2 / 60f);
            int seconds = Mathf.FloorToInt(lap2 % 60f);
            int hundredths = Mathf.FloorToInt((lap2 * 100f) % 100f);

            m_Lap2Text.text = $"LAP 2\n{minutes:00}:{seconds:00}:{hundredths:00}";
        }
        //lap 3
        if (m_RaceManager.LapTimes.Count > 2)
        {
            float lap3 = m_RaceManager.LapTimes[2];
            int minutes = Mathf.FloorToInt(lap3 / 60f);
            int seconds = Mathf.FloorToInt(lap3 % 60f);
            int hundredths = Mathf.FloorToInt((lap3 * 100f) % 100f);

            m_Lap3Text.text = $"LAP 3\n{minutes:00}:{seconds:00}:{hundredths:00}";
        }
        //newest best
        if (_newBestRaceTime)
        {
            if (_hadPreviousBest)
            {
                float difference = _oldBestRaceTime - raceTime;
                int minutediff = Mathf.FloorToInt(difference / 60f);
                int seconddiff = Mathf.FloorToInt(difference % 60f);
                int hundredthdiff = Mathf.FloorToInt((difference * 100f) % 100f);

                m_PersonalBestText.text = $"NEW PERSONAL BEST!\n" + $"{raceMinutes:00}:{raceSeconds:00}:{raceHundredths:00}\n" + $"-{minutediff:00}:{seconddiff:00}:{hundredthdiff:00}";
            }
            else
            {
                m_PersonalBestText.text = $"NEW PERSONAL BEST!\n" + $"{raceMinutes:00}:{raceSeconds:00}:{raceHundredths:00}";
            }
        }
        else
        {
            m_PersonalBestText.text = "";
        }
    }

    /// <summary>
    /// Simple function to handle making text popups that clear after a short space of time
    /// </summary>
    /// <param name="_popupText">The text to display</param>
    public void SpawnTextPopup(string _popupText)
    {
        GameObject popup = Instantiate(m_TextPopup, transform);
        popup.GetComponent<TextMeshProUGUI>().text = _popupText; // setting text after spawning
        Destroy(popup, m_PopupLifeLength); // passing in life length means they are deleted automatically after the time elapses
    }

    /// <summary>
    /// display temp feedback when player completes lap and include a star when new best lap is achieved
    /// </summary>
    public void ShowLapComplete(float _lapTime, bool _newBest)
    {
        StartCoroutine(LapCompletePopup(_lapTime, _newBest));
    }

    /// <summary>
    /// Function to display lap feedback
    /// </summary>
    private IEnumerator LapCompletePopup(float _lapTime, bool _newBest)
    {
        m_LapCompleteText.gameObject.SetActive(true);
        m_LapCompleteText.text = "LAP COMPLETE!";

        if (_newBest)
        {
            m_BestLapStar.SetActive(true);
        }

        yield return new WaitForSeconds(2.0f);
        m_LapCompleteText.gameObject.SetActive(false);
        m_BestLapStar.SetActive(false);
    }

    /// <summary>
    /// Editor-only function that is called when something changes, the scene starts, and 
    /// some other convenient locations/times
    /// </summary>
    private void OnValidate()
    {
        // Auto-attach text display from child
        if (!m_TextDisplay)
        {
            m_TextDisplay = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    /// <summary>
    /// briefly display go when countdown finishes
    /// </summary>
    private IEnumerator ShowGoText()
    {
        m_CountDownText.gameObject.SetActive(true);
        m_CountDownText.text = "GO!";
        yield return new WaitForSeconds(0.7f);
        m_CountDownText.gameObject.SetActive(false);
    }

    /// <summary>
    /// handle state when using escape key in different menus
    /// </summary>
    private void HandleEscape()
    {
        //no pausing after race is finished
        if (m_RaceManager.CurrentState == RaceManager.RaceState.FINISHED)
        {
            return;
        }

        //settings currently open
        if (m_gameSettingsMenu.activeSelf)
        {
            CloseGameSettings();
            return;
        }

        //pause menu currently open
        if (m_isPaused)
        {
            ResumeGame();
            return;
        }

        //game currently running
        PauseGame();
    }

    /// <summary>
    /// Function to stop time ingame and activate pause menu
    /// </summary>
    public void PauseGame()
    {
        m_isPaused = true;
        m_PauseMenu.SetActive(true);
        m_gameSettingsMenu.SetActive(false);
        Time.timeScale = 0.0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Our update loop from Unity, called as fast as possible 
    /// </summary>
    private void Update()
    {
        if (m_InputActions.FindAction("Pause").WasPressedThisFrame())
        {
            HandleEscape();
        }
        if (m_RaceManager.CurrentState != m_previousState)
        {
            if (m_RaceManager.CurrentState == RaceManager.RaceState.IN_RACE)
            {
                StartCoroutine(ShowGoText());
            }
            m_previousState = m_RaceManager.CurrentState;
        }
        // Reset text display
        m_TextDisplay.text = "";

        // State + state timer
        m_TextDisplay.text += $"State: {m_RaceManager.CurrentState} | {m_RaceManager.StateTimer}";

        // Respawn + respawn timer
        bool carRespawning = m_RaceManager.Respawning;
        m_TextDisplay.text += $"\nRespawning: {carRespawning}";
        if (carRespawning)
        {
            m_TextDisplay.text += $" | {m_RaceManager.RespawnTimer}";
        }
        
        // Car speed
        m_TextDisplay.text += $"\nSpeed: {m_PlayerCar.linearVelocity.magnitude} u/s";

        // Track lengths + percentage
        float completedTrackLength = m_RaceManager.CompletedTrackLength;
        float fullTrackLength = m_RaceManager.FullTrackLength;
        m_TextDisplay.text += $"\nDistance: {completedTrackLength} / {fullTrackLength} ({completedTrackLength / fullTrackLength * 100}%)";

        // Checkpoints + laps progress
        m_TextDisplay.text += $"\nCheckpoint: {m_RaceManager.CurrentCheckpoint} / {m_RaceManager.m_Checkpoints.Length}";
        m_TextDisplay.text += $"\nLap: ";
        if (m_RaceManager.HasFinishedRace()) 
        {
            m_TextDisplay.text += $"FINISHED";
        }
        else
        {
            m_TextDisplay.text += $"{m_RaceManager.CurrentLap} / {m_RaceManager.m_MaxLaps}";
        }

        // Current lap + race time
        m_TextDisplay.text += $"\nLap Time: {m_RaceManager.LapTime} s";
        m_TextDisplay.text += $"\nRace Time: {m_RaceManager.RaceTime} s";

        // Best (saved to PlayerPrefs) lap + race time if available
        m_TextDisplay.text += $"\nBest Lap Time: ";
        if (!PlayerPrefs.HasKey($"BestLapTime_{SceneManager.GetActiveScene().name}")) 
        {
            m_TextDisplay.text += "-";
        }
        else
        {
            m_TextDisplay.text += $"{PlayerPrefs.GetFloat($"BestLapTime_{SceneManager.GetActiveScene().name}")} s";
        }

        m_TextDisplay.text += $"\nBest Race Time: ";
        if (!PlayerPrefs.HasKey($"BestRaceTime_{SceneManager.GetActiveScene().name}")) 
        {
            m_TextDisplay.text += $"-";
        }
        else
        {
            m_TextDisplay.text += $"{PlayerPrefs.GetFloat($"BestRaceTime_{SceneManager.GetActiveScene().name}")} s";
        }

        //Lap info
        string bestLapKey = $"BestLapTime_{SceneManager.GetActiveScene().name}";
        if (!PlayerPrefs.HasKey(bestLapKey)){
            m_BestLapInfo.text = "BEST: 00:00:00";
            m_BestLapStar.SetActive(false);
        }
        else
        {
            float bestLap = PlayerPrefs.GetFloat(bestLapKey);
            int minutes = Mathf.FloorToInt(bestLap / 60);
            int seconds = Mathf.FloorToInt(bestLap % 60f);
            int hundredths = Mathf.FloorToInt((bestLap * 100f) % 100f);
            m_BestLapInfo.text = $"BEST: {minutes:00}:{seconds:00}:{hundredths:00}";
            
        }

        if (m_RaceManager.HasFinishedRace())
        {
            m_LapInfo.text = "FINISHED";
        }
        else
        {
            m_LapInfo.text = $"{m_RaceManager.CurrentLap}/{m_RaceManager.m_MaxLaps}";
        }

        float lapTime = m_RaceManager.LapTime;
        int lapMinutes = Mathf.FloorToInt(lapTime / 60f);
        int lapSeconds = Mathf.FloorToInt(lapTime % 60f);
        int lapHundredths = Mathf.FloorToInt((lapTime * 100f) % 100f);
        m_CurrentLapInfo.text = $"CURRENT: {lapMinutes:00}:{lapSeconds:00}:{lapHundredths:00}\n";

        //Race time info
        float raceTime = m_RaceManager.RaceTime;
        int raceMinutes = Mathf.FloorToInt(raceTime / 60f);
        int raceSeconds = Mathf.FloorToInt(raceTime % 60f);
        int raceHundredths = Mathf.FloorToInt((raceTime * 100f) % 100f);

        m_TimeInfo.text = $"{raceMinutes:00}:{raceSeconds:00}:{raceHundredths:00}\n";

        //Progress info
        //calculate track completion percentage and checkpoint progress
        float progress = 0.0f;
        if (fullTrackLength > 0.0f)
        {
            progress = completedTrackLength / fullTrackLength * 100.0f;
        }

        int currentCheckpoint = m_RaceManager.CurrentCheckpoint;
        int totalCheckpoints = m_RaceManager.m_Checkpoints.Length;

        m_ProgressInfo.text = $"{progress:0}%\n\n" + $"{currentCheckpoint}/{totalCheckpoints}";
        
        //speed
        float speed = m_PlayerCar.linearVelocity.magnitude;
        m_Speedometer.text = $"{speed:0}";

        //countdown
        if (m_RaceManager.CurrentState == RaceManager.RaceState.COUNTDOWN)
        {
            int countdownNumber = Mathf.CeilToInt(m_RaceManager.StateTimer);
            m_CountDownText.gameObject.SetActive(true);
            m_CountDownText.text = countdownNumber.ToString();
        }
        else if (m_RaceManager.CurrentState != RaceManager.RaceState.IN_RACE) {
            m_CountDownText.gameObject.SetActive(false);
        }

        //speedometer bars
        float playerSpeed = m_PlayerCar.linearVelocity.magnitude;
        //update speedometer segment based on players current speed
        float[] speedThresholds = { 3f, 6f, 9f, 12f, 15f };

        for (int i = 0; i < m_speedBars.Length; i++)
        {
            if (speed >= speedThresholds[i])
            {
                m_speedBars[i].color = m_speedActiveColor;
            }
            else
            {
                m_speedBars[i].color = m_speedInactiveColor;
            }
        }
    }

    /// <summary>
    /// open settings menu from paused menu
    /// </summary>
    public void OpenGameSettings()
    {
        m_PauseMenu.SetActive(false);
        m_gameSettingsMenu.SetActive(true);
    }

    /// <summary>
    /// close settings menu from paused menu
    /// </summary>
    public void CloseGameSettings()
    {
        m_gameSettingsMenu.SetActive(false);
        m_PauseMenu.SetActive(true);
    }
}
