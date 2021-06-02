using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
    [SerializeField] SettingsUI settingsUI;
    [SerializeField] GameObject mainScreenUI;
    [SerializeField] GameObject ghostModeUI;
    [SerializeField] GameObject leaderboardUI;
    private void Awake()
    {
        if (Instance != null)
        {
            //Destroy(this);
        } else
        {
            Instance = this;
        }
        Time.timeScale = 1;
    }

    public void StartGame()
    {
        LevelLoader.Instance.LoadScene(2);
    }

    public void ShowMainButtons(bool show)
    {
        mainScreenUI.SetActive(show);
    }

    public void OnClickPlay()
    {
        ghostModeUI.SetActive(true);
        ShowMainButtons(false);
    }

    public void OpenSettings(bool open)
    {
        settingsUI.Open(open);
    }

    public void OnClickLeaderboard()
    {
        leaderboardUI.SetActive(true);
        ShowMainButtons(false);
    }

    public void OnClickBack()
    {
        if (leaderboardUI.activeInHierarchy)
        {
            leaderboardUI.SetActive(false);
            ShowMainButtons(true);
        }
    }

    public void OnClickGarage()
    {
        GarageManager.Instance.OpenGarage(true);
        ShowMainButtons(false);
    }
}
