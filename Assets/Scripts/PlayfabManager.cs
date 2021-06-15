using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager Instance;
    string username;
    string playfabID;

    public event Action<bool> OnUserLoggedIn;

    public const int largeNumToSubtract = 1000;

    private const string customIDKey = "CustomID";
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        } else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        Login();
    }

    public string GetPlayfabID()
    {
        return playfabID;
    }
    
    void Login()
    {
        LoginWithCustomIDRequest request;
        string customID;

        #if UNITY_WEBGL
            if(!PlayerPrefs.HasKey(customIDKey))
            {
                customID = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(customIDKey, customID);
            }
            else
            {
                customID = PlayerPrefs.GetString(customIDKey);
            }
        #else
            customID = SystemInfo.deviceUniqueIdentifier;
        #endif
        request = new LoginWithCustomIDRequest
        {
            CustomId = customID,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetPlayerStatistics = true,
                GetUserAccountInfo = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);
    }

    void OnSuccess(LoginResult result)
    {
        if (result.NewlyCreated)
        {
            Debug.Log("Successfuly created new account. ID: " + result.PlayFabId);
        } else
        {
            Debug.Log("Successfully logged in player with playfab ID of: " + result.PlayFabId);
        }

        playfabID = result.PlayFabId;
        string name = null;
        if (result.InfoResultPayload.PlayerProfile != null)
        {
            name = result.InfoResultPayload.PlayerProfile.DisplayName;
        }

        if (name != null)
        {
            Debug.Log("Player username: " + name);
            username = name;
        } else
        {
            Debug.Log("Player username is null");
            username = null;
        }

        SettingsUI.Instance.SetUsername(username);

        // invoke the event if it has been subscribed to
        OnUserLoggedIn?.Invoke(true);
        //GetLeaderboard();
        //SendScore(100000);
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.Error);
        Debug.LogError("Error Message: " + error.ErrorMessage);
        Debug.LogError("Error details: " + error.ErrorDetails);
    }

    public void TryUpdatingUsername(string name)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = name
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUserNameSuccessfullyUpdated, OnUserNameUpdateFailed);
    }

    public void OnUserNameUpdateFailed(PlayFabError error)
    {
        Debug.Log("Username change failed. Error: " + error.GenerateErrorReport());
        if (SettingsUI.Instance.IsOpen)
        {
            SettingsUI.Instance.OnUserNameUpdateFailed(error);
        } else
        {
            RacingUIManager.Instance.OnUpdateUsernameFailed(error);
        }
    }

    public void OnUserNameSuccessfullyUpdated(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Successfully changed username to " + result.DisplayName);
        username = result.DisplayName;

        if (SettingsUI.Instance.IsOpen)
        {
            SettingsUI.Instance.OnUserNameSuccessfullyUpdated(result);
        }
        else
        {
            RacingUIManager.Instance.OnUserNameSuccessfullyUpdated(result);
        }
    }

    public void SendScore(float time)
    {
        time = (int)(time * 1000);
        time /= 1000;
        float secsFromLargeNum = (float)largeNumToSubtract - time;

        int milliseconds = (int)(secsFromLargeNum * 1000);

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = SceneChangeManager.Instance.GetCurrentSceneName() + CarModelManager.Instance.GetCurrentCarName(),
                    Value = milliseconds
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnLeaderboardError);
    }

    public static float ScoreToSeconds(float score)
    {
        if (score == 0)
        {
            return 3599.999f;
        }
        float seconds = (float)score / 1000;
        return (float)largeNumToSubtract - seconds;
    }

    public void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successful leaderboard sent");

        GetLeaderboard();
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = SceneChangeManager.Instance.GetCurrentSceneName() + CarModelManager.Instance.GetCurrentCarName(),
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnLeaderboardError);
    }

    public void GetPlayerLeaderboardSlot(string map, string carName)
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = map + carName,
            MaxResultsCount = 1
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetLeaderboardAroundPlayer, OnLeaderboardError);
    }

    void OnGetLeaderboardAroundPlayer(GetLeaderboardAroundPlayerResult result)
    {
        // call method on leaderboard manager to set the last data value to this player's score and position
        if (LeaderboardUIManager.Instance != null)
        {
            LeaderboardUIManager.Instance.GetCurrentPlayerPlacement(result);
        }
    }

    public void GetLeaderboard(string map, string carName)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = map + carName,
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnLeaderboardError);    
    }

    public void OnLeaderboardGet(GetLeaderboardResult result)
    {
        if (LeaderboardUIManager.Instance != null)
        {
            LeaderboardUIManager.Instance.SetLeaderboard(result);
        }

        //for(int i = 0; i < result.Leaderboard.Count; i++)
        //{
        //    var data = result.Leaderboard[i];
        //    Debug.Log(data.Position + " " + data.DisplayName + " " + ((float)data.StatValue / 100));
        //}
    }

    public void OnLeaderboardError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void GetLeaderboardTime()
    {
        GetPlayerStats();
    }

    public void GetPlayerStats()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = SceneChangeManager.Instance.GetCurrentSceneName() + CarModelManager.Instance.GetCurrentCarName(),
            MaxResultsCount =1
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetPlayerStats, OnLeaderboardError);
    }

    void OnGetPlayerStats(GetLeaderboardAroundPlayerResult result)
    {
        Debug.Log(result.Leaderboard[0].DisplayName);
        RacingUIManager.Instance.SetPersonalBestTime(ScoreToSeconds(result.Leaderboard[0].StatValue));
    }

    public string GetUsername()
    {
        if (username == null)
        {
            return null;
        }
        return username;
    }

}
