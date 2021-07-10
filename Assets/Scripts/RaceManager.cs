using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RaceManager : MonoBehaviourPunCallbacks
{
    public static RaceManager Instance;
    //public bool hasRaceStarted = false;

    public AudioSource audioSource;
    public AudioClip countdownAudioClip;

    public bool disableCountdown = false;
    public bool startedRace = false;
    CarInput carInput;

    public int vSyncCount = 0;
    public int frameRateLimit = 60;

    RaceInformation raceInfo;

    float startCountdownDelay = 1;

    bool isCountdownPaused = false;

    int lapsToFinish = 1;
    int currentLaps;
    bool playerFinished = false;
    bool ghostFinished = false;

    bool allPlayersAreReady = false;

    public Animator countdownAnimator;

    Player[] playerList;
    //bool ghostFinished = false;


    public override void OnEnable()
    {
        base.OnEnable();
        PhotonEventHolder.OnAllPlayersLoaded += PrepareCountdown;
        CheckpointManager.OnCompletedLap += OnCompletedLap;
        CheckpointManager.OnFinishedRace += OnFinishedRace;
        Time.timeScale = 1;
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        CheckpointManager.OnCompletedLap -= OnCompletedLap;
        CheckpointManager.OnFinishedRace -= OnFinishedRace;
        PhotonEventHolder.OnAllPlayersLoaded -= PrepareCountdown;
    }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }

        // set this to null because it should've have a reference in this scene.
        LeaderboardUIManager.Instance = null;

        raceInfo = ReferenceManager.Instance.GetRaceInformation();
    }


    private void Start()
    {
        carInput = CarSpawner.Instance.GetCurrentCar().GetComponent<CarInput>();

        // if playing multiplayer
        if (raceInfo.mode == Mode.Multiplayer)
        {
            carInput.EnableInput(false);

            // set this player status to loaded level
            Hashtable hashtable = new Hashtable { { PlayerCustomProperties.PlayerLoadedLevel, true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
            playerList = PhotonNetwork.PlayerList;
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PlayerCustomProperties.PlayerLoadedLevel, out object loaded);
            Debug.Log((bool)loaded);
            if (PhotonNetwork.IsMasterClient)
            {
                if (AllPlayersLoaded())
                {
                    RaiseStartCountdownEvent();
                    //StartCountdown();
                }
            }

        }
        else
        {
            if (!disableCountdown)
            {
                carInput.EnableInput(false);

                Invoke(nameof(StartCountdown), startCountdownDelay);
            }
            else
            {
                StartRace();
            }
        }    
    }

    #region Multiplayer Stuff

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log("Player properties update");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log(targetPlayer.NickName + " changed props.");
            if (AllPlayersLoaded())
            {
                RaiseStartCountdownEvent();
                //StartCountdown();
            }
        }      
    }

    bool AllPlayersLoaded()
    {
        Debug.Log("Player list count: " + playerList.Length);
        
        foreach (Player p in playerList)
        {
            p.CustomProperties.TryGetValue(PlayerCustomProperties.PlayerLoadedLevel, out object loaded);
            Debug.Log(p.NickName + " loaded: " + loaded);
            if (loaded == null || !((bool)loaded))
            {
                allPlayersAreReady = false;
                return false;
            }
        }
        allPlayersAreReady = true;
        return true;     
    }

    void RaiseStartCountdownEvent()
    {
        Debug.Log("Raise event");
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions()
        { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal};

        double startTime = PhotonNetwork.Time;
        startTime += 2;
        PhotonNetwork.RaiseEvent(PhotonEvents.AllPlayersLoaded, startTime, raiseEventOptions, ExitGames.Client.Photon.SendOptions.SendReliable);
    }


    

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerList = PhotonNetwork.PlayerList;
    }

    void PrepareCountdown(double startTime)
    {
        Debug.Log("Start at " + startTime);
        Invoke(nameof(StartCountdown), (float)startTime - (float)PhotonNetwork.Time);
    }
    #endregion



    public void OnGhostFinished()
    {
        ghostFinished = true;
        if (!playerFinished)
        {
            RacingUIManager.Instance.OnPlayerLost();
            Debug.Log("Player lost");
        }
    }

    public void OnRestartGamePressed()
    {
        SceneChangeManager.Instance.RestartScene();
    }

    private void Update()
    {
        if (!startedRace)
        {
            carInput.EnableInput(false);
        }
        if (RacingUIManager.Instance.isPaused)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                isCountdownPaused = true;
            }
            return;
        } else if (isCountdownPaused)
        {
            audioSource.Play();
            isCountdownPaused = false;
        }
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            frameRateLimit -= 10;
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            frameRateLimit += 10;
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            frameRateLimit = 30;
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            frameRateLimit = 144;
        }
        Application.targetFrameRate = frameRateLimit;

        if (Keyboard.current[Key.V].wasPressedThisFrame)
        {
            QualitySettings.vSyncCount = (QualitySettings.vSyncCount == 0) ? 1 : 0;
        }

        //if (Keyboard.current[Key.G].wasPressedThisFrame)
        //{
            
        //}

        if (Time.timeScale == 0)
        {
            
        } if (Time.timeScale != 0 && isCountdownPaused)
        {
            audioSource.Play();
            isCountdownPaused = false;
        }
    }
    void StartCountdown()
    {
        audioSource.clip = countdownAudioClip;
        audioSource.Play();
        countdownAnimator.SetTrigger("StartCountdown");
        Invoke(nameof(StartRace), countdownAudioClip.length - 0.23f);
    }

    public void StartRace()
    {
        startedRace = true;
        LapTimeManager.Instance.StartTimer();
        carInput.EnableInput(true);
        GhostCapture.Instance.StartGhost();
    }

    public void OnCompletedLap()
    {
        
    } 

    public void OnFinishedRace()
    {
        Time.timeScale = 0.4f;
        playerFinished = true;

        if (raceInfo.mode == Mode.GhostRace)
        {
            PlayfabManager.Instance.SendScore(LapTimeManager.Instance.GetTime());

            bool isUsingCybertrueno = CarModelManager.Instance.GetCarNumber() == 0;

            // if the player beat the ghost in hard or insane mode
            if (playerFinished && !ghostFinished && (int)raceInfo.ghostDifficulty >= (int)Difficulty.Hard || (isUsingCybertrueno && (int)raceInfo.ghostDifficulty >= (int)Difficulty.Medium))
            //&& (raceInfo.ghostDifficulty == Difficulty.Hard || raceInfo.ghostDifficulty == Difficulty.Insane 
            //|| (isUsingCybertrueno && raceInfo.ghostDifficulty == Difficulty.Medium)))
            {
                CarInventoryManager.Instance.TryUnlockCar(CarModelManager.Instance.GetCarNumber());
            }
            //Debug.Log("Try unlock car");
        } else
        {
            // send score for multiplayer leaderboard
        }
    }

} // end of class
