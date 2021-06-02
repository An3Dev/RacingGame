using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class RacingUIManager : MonoBehaviour
{
    public static RacingUIManager Instance;
    public TextMeshProUGUI timer;
    public TextMeshProUGUI speed;
    public TextMeshProUGUI resultsMessageText;
    public Image speedImage;
    public TextMeshProUGUI speedUnitsText;
    CarController carController;
    public TMP_InputField usernameInputField;
    public TextMeshProUGUI usernameSetResult, countdownText, personalBestText, endScreenPersonalBestText;

    public GameObject racingPanel, pausedPanel, lostRacePanel, userNamePopup;

    [Header("EndScreen")]
    public GameObject endScreenPanel;
    public TextMeshProUGUI endScreenTime, fps;

    bool useMilesPerHour = true;

    float personalBestTime;

    public bool isPaused { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }
        //SetTimerText(61.5f);

        InvokeRepeating(nameof(UpdateSpeedText), 0.001f, 0.05f);
        PlayfabManager.Instance.OnUserLoggedIn += GetLeaderboardTime;
    }

    private void Start()
    {
        carController = CarSpawner.Instance.GetCurrentCar().GetComponent<CarController>();

        // when the miles change, change the boolean
        SettingsUI.Instance.OnUseMilesChanged += (use) => UpdateUseMiles(use);

        useMilesPerHour = SettingsUI.Instance.useMiles;

        if (useMilesPerHour)
        {
            speedUnitsText.text = "MPH";
        } else
        {
            speedUnitsText.text = "KPH";
        }
    }

    void GetLeaderboardTime(bool b)
    {
        PlayfabManager.Instance.GetLeaderboardTime();
    }

    public void SetPersonalBestTime(float seconds)
    {
        personalBestTime = seconds;
        personalBestText.text = "PB: " + LapTimeManager.GetLapTimeString(seconds);

        endScreenPersonalBestText.text = "PB: " + LapTimeManager.GetLapTimeString(seconds);
    }

    void UpdateUseMiles(bool use)
    {
        useMilesPerHour = use;
        if (useMilesPerHour)
        {
            speedUnitsText.text = "MPH";
        }
        else
        {
            speedUnitsText.text = "KPH";
        }
    }

    public void SetCountdownText(string text)
    {
        countdownText.text = text;
    }


    private void Update()
    {
        if (SettingsUI.Instance.ShowFps)
        {
            if (!fps.gameObject.activeInHierarchy)
                fps.gameObject.SetActive(true);
                
            fps.text = ((int)(1 / Time.smoothDeltaTime)).ToString();
        } else
        {
            if (fps.gameObject.activeInHierarchy)
                fps.gameObject.SetActive(false);
        }

        // if the user presses escape
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // if settings isn't open
            if (!SettingsUI.Instance.IsOpen)
            {
                // toggle pause
                OpenPausePanel(!pausedPanel.activeInHierarchy);
            }
        }
    }

    public void OnUnpause()
    {
        carController.PausedGame(false);
        isPaused = false;
    }

    public void OnPlayerLost()
    {
        lostRacePanel.SetActive(true);
        resultsMessageText.text = GhostModeManager.currentDifficulty.ToString() + " Ghost Beat You!";
    }

    void OpenPausePanel(bool open)
    {
        pausedPanel.SetActive(open);
        racingPanel.SetActive(!open);
        isPaused = open;
        if (!open)
        {
            Time.timeScale = 1f;
        } else
        {
            Time.timeScale = 0f;
        }

        if (!open)
        {
            OnUnpause();
        } else {
            carController.PausedGame(true);
        }
    }

    public void OnClickResume()
    {
        OpenPausePanel(false);
        OnUnpause();
    }

    public void OnSettingsButtonPressed()
    {
        SettingsUI.Instance.Open(true);
    }

    void UpdateSpeedText()
    {
        speedImage.fillAmount = carController.GetSpeed() / carController.GetMaxSpeed() / 2; // divide by 2 because the image fills at 0.5
        float metersPerSec = carController.GetSpeed();
        speed.text = ((int)(metersPerSec * 3.6f * (useMilesPerHour ? 0.62137f : 1))).ToString("00");
    }

    public void SetTimerText(string formattedText)
    {     
        timer.text = formattedText;
    }

    public void OnClickMainMenuButton()
    {
        SceneChangeManager.Instance.LoadScene(SceneChangeManager.Instance.mainMenuBuildIndex);
    }

    public void OnClickRestartButton()
    {
        SceneChangeManager.Instance.RestartScene();
    }

    public void OnCompletedLap()
    {
        ShowEndScreen(true);
        ShowRacingScreen(false);
        isPaused = true;
        if (PlayfabManager.Instance.GetUsername() == null || PlayfabManager.Instance.GetUsername().Length < 5)
        {
            Debug.Log("Show popup");
            userNamePopup.SetActive(true);
        }
    }

    public void OnClickSubmitUsername()
    {
        if (usernameInputField.text.Length < 3)
        {
            usernameSetResult.gameObject.SetActive(true);
            usernameSetResult.text = "Username must be at least 3 characters long";
            return;
        }
        PlayfabManager.Instance.TryUpdatingUsername(usernameInputField.text);
    }

    public void OnUpdateUsernameFailed(PlayFabError error)
    {
        usernameSetResult.gameObject.SetActive(true);
        usernameSetResult.text = "Username set failed. Error: " + error;
    }

    public void OnUserNameSuccessfullyUpdated(UpdateUserTitleDisplayNameResult result)
    {
        userNamePopup.SetActive(false);
    }

    public void ShowEndScreen(bool show)
    {
        endScreenPanel.SetActive(show);
        endScreenTime.text = timer.text;
    }

    public void ShowRacingScreen(bool show)
    {
        racingPanel.SetActive(show);
        endScreenPersonalBestText.text = "PB: " + LapTimeManager.GetLapTimeString(personalBestTime);

    }
}
