using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
public class LeaderboardUIManager : MonoBehaviour
{
    public static LeaderboardUIManager Instance;
    public Transform carButtonParent;
    public Transform mapButtonParent;
    public Transform leaderboardEntryContainer;
    public TextMeshProUGUI infoText;

    public Color thisPlayerScoreTextColor, otherPlayerTextColor;

    ObjectSelectButton[] carButtons, mapButtons;

    Transform[] leaderboardEntries;

    int mapIndex = 0;
    int currentCarIndex;

    GetLeaderboardResult leaderboardResult;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        
    }

    void OnPlayerLoggedIn(bool loggedIn)
    {
        GetLeaderboard();
    }

    public void GetLeaderboard()
    {
        leaderboardResult = null;
        // disable all entries
        for (int i = 0; i < leaderboardEntries.Length; i++)
        {
            leaderboardEntries[i].gameObject.SetActive(false); //disables visual studio
        }

        PlayfabManager.Instance.GetLeaderboard(mapButtons[mapIndex].objectName, carButtons[currentCarIndex].objectName);
        infoText.gameObject.SetActive(true);
        infoText.text = "Retrieving leaderboard...";
    }

    // this is called by playfab manager whenever this class calls GetLeaderboard
    public void SetLeaderboard(GetLeaderboardResult leaderboard)
    {
        leaderboardResult = leaderboard;
        infoText.gameObject.SetActive(false);
        RefreshLeaderboard();
    }

    void RefreshLeaderboard()
    {
        string playfabID = PlayfabManager.Instance.GetPlayfabID();
        bool isPlayerInTop10 = false;
        if (leaderboardResult.Leaderboard.Count == 0)
        {
            Debug.Log("No data");
            infoText.gameObject.SetActive(true);
            infoText.text = "There aren't any scores yet. Be the first to complete a lap!";
            // disable all entries
            for (int i = 0; i < leaderboardEntries.Length; i++)
            {
                leaderboardEntries[i].gameObject.SetActive(false);
            }
            return;
        }
        else
        {
            infoText.gameObject.SetActive(false);
        }

        for (int i = 0; i < leaderboardResult.Leaderboard.Count; i++)
        {
            leaderboardEntries[i].gameObject.SetActive(true);
            TextMeshProUGUI[] text = leaderboardEntries[i].GetComponentsInChildren<TextMeshProUGUI>();

            text[0].text = (leaderboardResult.Leaderboard[i].Position + 1).ToString();

            if (leaderboardResult.Leaderboard[i].DisplayName != null && leaderboardResult.Leaderboard[i].DisplayName.Length > 0)
            {
                text[1].text = leaderboardResult.Leaderboard[i].DisplayName;
            }
            else
            {
                text[1].text = "No Username";
            }

            string time = LapTimeManager.GetLapTimeString(PlayfabManager.ScoreToSeconds(leaderboardResult.Leaderboard[i].StatValue));

            if (leaderboardResult.Leaderboard[i].StatValue == 0)
            {
                text[2].text = "N/A";
            }
            else
            {
                text[2].text = time;
            }

            if (leaderboardResult.Leaderboard[i].PlayFabId.Equals(playfabID))
            {
                isPlayerInTop10 = true;
                text[0].color = thisPlayerScoreTextColor;
                text[1].color = thisPlayerScoreTextColor;
                text[2].color = thisPlayerScoreTextColor;
            }
            else
            {
                text[0].color = otherPlayerTextColor;
                text[1].color = otherPlayerTextColor;
                text[2].color = otherPlayerTextColor;
            }
        }

        if (!isPlayerInTop10)
        {
            // get leaderboard around player
            PlayfabManager.Instance.GetPlayerLeaderboardSlot(mapButtons[mapIndex].objectName, carButtons[currentCarIndex].objectName);
        }
        
        // disable leaderboard entries if there isn't more data to show
        if (leaderboardResult.Leaderboard.Count < leaderboardEntries.Length)
        {
            for(int i = leaderboardResult.Leaderboard.Count; i < leaderboardEntries.Length; i++)
            {
                leaderboardEntries[i].gameObject.SetActive(false);
            }
        }
    }

    // this is called by Playfab Manager when it retrieves the leaderboard result of the player that's logged in currently.
    public void GetCurrentPlayerPlacement(GetLeaderboardAroundPlayerResult result)
    {
        //Debug.Log(result.Leaderboard[0].Position + " " + result.Leaderboard[0].DisplayName + " " + result.Leaderboard[0].StatValue);
        // sets the last entry UI to the current player's stats

        if (result.Leaderboard[0].StatValue == 0)
        {
            return;
        }
        Transform entry = leaderboardEntries[leaderboardEntries.Length - 1];
        entry.gameObject.SetActive(true);
        TextMeshProUGUI[] text = entry.GetComponentsInChildren<TextMeshProUGUI>();

        text[0].text = (result.Leaderboard[0].Position + 1).ToString();
        text[1].text = result.Leaderboard[0].DisplayName;

        string time = LapTimeManager.GetLapTimeString(PlayfabManager.ScoreToSeconds(result.Leaderboard[0].StatValue));
        if (result.Leaderboard[0].StatValue == 0)
        {
            text[2].text = "N/A";
        }
        else
        {
            text[2].text = time;
        }

        if (result.Leaderboard[0].DisplayName != null && result.Leaderboard[0].DisplayName.Length > 0)
        {
            text[1].text = result.Leaderboard[0].DisplayName;
        }
        else
        {
            text[1].text = "No Username";
        }

        // set this color to red
        text[0].color = thisPlayerScoreTextColor;
        text[1].color = thisPlayerScoreTextColor;
        text[2].color = thisPlayerScoreTextColor;
    }

    public void OnCarButtonClicked(int index)
    {
        Debug.Log(carButtons[index].objectName);
        currentCarIndex = index;
        // disable the selection images
        for(int i = 0; i < carButtons.Length; i++)
        {
            if (i != index)
            {
                carButtons[i].SetSelected(false);
            }
        }

        // show leaderboard based on the car name and map
        GetLeaderboard();
    }

    private void OnEnable()
    {
        carButtons = carButtonParent.GetComponentsInChildren<ObjectSelectButton>();
        mapButtons = mapButtonParent.GetComponentsInChildren<ObjectSelectButton>();

        leaderboardEntries = new Transform[leaderboardEntryContainer.childCount];
        for (int i = 0; i < leaderboardEntries.Length; i++)
        {
            leaderboardEntries[i] = leaderboardEntryContainer.GetChild(i);
        }

        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayfabManager.OnUserLoggedIn += OnPlayerLoggedIn; //like and subscribe
        }
        else
        {
            OnPlayerLoggedIn(true);
        }
        // toggle pause
    }
}
