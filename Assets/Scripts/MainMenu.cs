using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
    [SerializeField] SettingsUI settingsUI;
    [SerializeField] GameObject mainScreenUI;
    [SerializeField] GameObject ghostModeUI;
    [SerializeField] GameObject leaderboardUI;
    [SerializeField] GameObject controlsUI;
    [SerializeField] GameObject gameModeSelectionUI;

    GameObject[] panels;
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

        PhotonNetwork.Disconnect();
    }

    public void StartGame()
    {
        int buildIndex = 2;

        // should get the selected map here. Get its name.
        // get the build number based on the name.        

        LevelLoader.Instance.LoadScene(buildIndex);
    }

    public void ShowMainButtons(bool show)
    {
        mainScreenUI.SetActive(show);
    }

    public void OnClickPlay()
    {
        gameModeSelectionUI.SetActive(true);
        ShowMainButtons(false);
    }

    public void OpenSettings(bool open)
    {
        if (!open)
        {
            settingsUI.transform.GetChild(0).gameObject.SetActive(false);
        } else
        {
            settingsUI.Open(open);

        }
        ShowMainButtons(!open);
    }

    public void OpenGameModeUI(bool open)
    {
        gameModeSelectionUI.SetActive(open);
    }

    public void OpenGhostLevelSelection(bool open)
    {
        PhotonNetwork.OfflineMode = true;
        ghostModeUI.SetActive(open);
        gameModeSelectionUI.SetActive(!open);
    }

    public void OnClickMultiplayer()
    {
        PhotonNetwork.OfflineMode = false;
        SceneManager.LoadScene("MultiplayerMenu");
    }

    public void OpenControls(bool open)
    {
        controlsUI.SetActive(open);
        ShowMainButtons(!open);
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
        } else if (ghostModeUI.activeInHierarchy)
        {
            ghostModeUI.SetActive(false);
        } else if (controlsUI.activeInHierarchy)
        {
            controlsUI.SetActive(false);         
        } else if (gameModeSelectionUI.activeInHierarchy)
        {
            gameModeSelectionUI.SetActive(false);
        }
        ShowMainButtons(true);
    }

    public void OnClickGarage()
    {
        GarageManager.Instance.OpenGarage(true);
        ShowMainButtons(false);
    }
}
