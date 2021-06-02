using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
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

    float startCountdownDelay = 1;

    bool isCountdownPaused = false;

    int lapsToFinish = 1;
    int currentLaps;
    bool playerFinished = false;

    public Animator countdownAnimator;
    //bool ghostFinished = false;
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
    }


    private void Start()
    {
        carInput = CarSpawner.Instance.GetCurrentCar().GetComponent<CarInput>();
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

    private void OnEnable()
    {
        Time.timeScale = 1;
    }

    public void OnGhostFinished()
    {
        if (!playerFinished)
        {
            RacingUIManager.Instance.OnPlayerLost();
            Debug.Log("Player lost");
        }
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
        if (Keyboard.current[Key.G].wasPressedThisFrame)
        {
            SceneChangeManager.Instance.RestartScene();
        }

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
        currentLaps++;
        if (currentLaps >= lapsToFinish)
        {
            Time.timeScale = 0.4f;
            playerFinished = true;
        }

        PlayfabManager.Instance.SendScore(LapTimeManager.Instance.GetTime());

        // tell racing UI to show race against better ghost level button
        // send times to high score
    }
}
