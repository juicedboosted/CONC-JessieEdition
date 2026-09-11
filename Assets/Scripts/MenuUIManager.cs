using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [Header("External Components")]
    public InputActionAsset m_InputActions;
    
    [Header("Internal Components")]
    public TextMeshProUGUI m_ScoreDisplay;

    [SerializeField] private GameObject m_titleScreen;
    [SerializeField] private GameObject m_levelSelect;

    [SerializeField] private CanvasGroup m_titleCanvasGroup;
    [SerializeField] private CanvasGroup m_levelSelectCanvasGroup;
    [SerializeField] private float m_fadeDuration = 0.2f;

    private int m_selectedLevel = 1;

    [Header("Level Preview")]
    [SerializeField] private Image m_levelPreview;
    [SerializeField] private Sprite[] m_levelPreviewSprites;

    [SerializeField] private TMP_Text[] m_levelTexts;
    [SerializeField] private Color m_selectedColor = new Color(1f, 0.55f, 0f);
    [SerializeField] private Color m_normalColor = Color.white;

    [SerializeField] private GameObject m_clearConfirm;
    [SerializeField] private GameObject m_mainSettingsPanel;

    /// <summary>
    /// Function to quit
    /// </summary>
    public void OnExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Function to move from title screen state to level select state
    /// </summary>
    public void ShowLevelSelect()
    {
        StartCoroutine(SwitchScreen(m_titleScreen, m_titleCanvasGroup, m_levelSelect, m_levelSelectCanvasGroup));

    }

    /// <summary>
    /// Function to move from level select state to title screen state
    /// </summary>
    public void ShowTitleScreen()
    {
        StartCoroutine(SwitchScreen(m_levelSelect, m_levelSelectCanvasGroup, m_titleScreen, m_titleCanvasGroup));
        //m_levelSelect.SetActive(false);
        //m_titleScreen.SetActive(true);
    }

    /// <summary>
    /// Function to switch screens (titlescreen to level select and vice versa) with a fade 
    /// </summary>
    private IEnumerator SwitchScreen(GameObject _currentScreen, CanvasGroup _currentGroup, GameObject _nextScreen, CanvasGroup _nextGroup)
    {
        //fade current screen out
        yield return StartCoroutine(FadeCanvasGroup(_currentGroup, 1f, 0));
        _currentScreen.SetActive(false);

        //next screen enabled
        _nextScreen.SetActive(true);
        _nextGroup.alpha = 0f;

        UpdateLevelSelection();
        DisplayScores();

        //fade next screen in
        yield return StartCoroutine(FadeCanvasGroup(_nextGroup, 0f, 1f));
    }

    /// <summary>
    /// Function to change the alpha of a group to give a fade effect
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup _canvasGroup, float _startAlpha, float _endAlpha)
    {
        //no clicking
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        float time = 0f;
        while (time < m_fadeDuration)
        {
            time += Time.deltaTime;
            float percentage = time / m_fadeDuration;
            _canvasGroup.alpha = Mathf.Lerp(_startAlpha, _endAlpha, percentage);
            yield return null;
        }
        _canvasGroup.alpha = _endAlpha;
        //yes clicking
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Function to set the selected level using user input
    /// </summary>
    public void SelectLevel(int _level)
    {
        m_selectedLevel = _level;

        UpdateLevelSelection();
        DisplayScores();
    }

    /// <summary>
    /// Update visual state of level selection menu, highlight selected level and display preview image
    /// </summary>
    public void UpdateLevelSelection()
    {
        for (int i = 0; i < m_levelTexts.Length; i++)
        {
            if (i == m_selectedLevel - 1)
            {
                m_levelTexts[i].color = m_selectedColor;
            }
            else
            {
                m_levelTexts[i].color = m_normalColor;
            }
        }

        //change level preview image
        if (m_levelPreviewSprites.Length >= m_selectedLevel)
        {
            m_levelPreview.sprite = m_levelPreviewSprites[m_selectedLevel - 1];
        }
    }

    /// <summary>
    /// Function to load the scene of the player's selected level
    /// </summary>
    public void PlayLevel()
    {
        SceneManager.LoadScene("Level" + m_selectedLevel);
    }

    /// <summary>
    /// Function to retrieve levels best lap and race times from playerprefs and format them into the UI
    /// </summary>
    private void DisplayScores()
    {
        int level = m_selectedLevel;
        m_ScoreDisplay.text = "";
        m_ScoreDisplay.text += "BEST LAP\n";

        if (!PlayerPrefs.HasKey($"BestLapTime_Level{level}"))
        {
            m_ScoreDisplay.text += "--:--:--";
        }
        else
        {
            float lapTime = PlayerPrefs.GetFloat($"BestLapTime_Level{level}");
            int minutes = Mathf.FloorToInt(lapTime / 60f);
            int seconds = Mathf.FloorToInt(lapTime % 60f);
            int hundredths = Mathf.FloorToInt((lapTime * 100f) % 100f);

            m_ScoreDisplay.text += $"{minutes:00}:{seconds:00}:{hundredths:00}";
        }

        m_ScoreDisplay.text += "\n\nBEST RACE\n";

        if (!PlayerPrefs.HasKey($"BestRaceTime_Level{level}"))
        {
            m_ScoreDisplay.text += "--:--:--";
        }
        else
        {
            float raceTime = PlayerPrefs.GetFloat($"BestRaceTime_Level{level}");
            int minutes = Mathf.FloorToInt(raceTime / 60f);
            int seconds = Mathf.FloorToInt(raceTime % 60f);
            int hundredths = Mathf.FloorToInt((raceTime * 100f) % 100f);

            m_ScoreDisplay.text += $"{minutes:00}:{seconds:00}:{hundredths:00}";
        }
    }

    /// <summary>
    /// Function to activate the clear data confirm prompt
    /// </summary>
    public void ShowClearConfirm()
    {
        m_clearConfirm.SetActive(true);
    }

    /// <summary>
    /// Function to deactivate the clear data confirm prompt
    /// </summary>
    public void HideClearConfirm()
    {
        m_clearConfirm.SetActive(false);
    }

    /// <summary>
    /// Clear all saved race records and refresh score display
    /// </summary>
    public void ConfirmClearData()
    {
        PlayerPrefs.DeleteAll();
        DisplayScores();
        m_clearConfirm.SetActive(false);
    }

    /// <summary>
    /// open settings menu from main menu
    /// </summary>
    public void OpenMainSettings()
    {
        m_levelSelect.SetActive(false);
        m_mainSettingsPanel.SetActive(true);
    }

    /// <summary>
    /// close settings menu from main menu
    /// </summary>
    public void CloseGameSettings()
    {
        m_mainSettingsPanel.SetActive(false);
        m_levelSelect.SetActive(true);
    }

    /// <summary>
    /// Called when this gameObject is first made active
    /// </summary>
    private void Start()
    {
        // Unlock mouse cursor because it can be left locked from the game scene!
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Our update loop from Unity, called as fast as possible 
    /// </summary>
    private void Update()
    {
        // Use input to trigger exit as well
        if (m_InputActions.FindAction("Quit").WasPressedThisFrame())
        {
            OnExitGame();
        }
    }
}
