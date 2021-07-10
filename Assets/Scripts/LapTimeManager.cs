using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapTimeManager : MonoBehaviour
{
    public static LapTimeManager Instance;
    RaceInformation raceInfo;
    bool isTimeRunning = false;

    float timeInSeconds = 0;
    int numLaps = 0;

    List<float> lapTimesList= new List<float>();

    int minutes;
    int seconds;
    int milliseconds;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }
        raceInfo = ReferenceManager.Instance.GetRaceInformation();
    }

    private void OnEnable()
    {
        CheckpointManager.OnCompletedLap += OnCompletedLap;
        CheckpointManager.OnFinishedRace += OnFinishedRace;
    }

    private void OnDisable()
    {
        CheckpointManager.OnCompletedLap -= OnCompletedLap;
        CheckpointManager.OnFinishedRace -= OnFinishedRace;
    }

    public float GetTime()
    {
        return timeInSeconds;
    }

    public void StartTimer()
    {
        isTimeRunning = true;
        StartCoroutine("RunTimer");
    }

    public int GetNumLaps()
    {
        return numLaps;
    }

    public void StopTimer()
    {
        isTimeRunning = false;
        StopCoroutine("RunTimer");
    }

    public void OnCompletedLap()
    {
        float totalSecondsBeforeThisLap = 0;
        for (int i = 0; i < lapTimesList.Count; i++)
        {
            totalSecondsBeforeThisLap += lapTimesList[i];
        }

        lapTimesList.Add(seconds - totalSecondsBeforeThisLap);

        // tell user their lap time
        Debug.Log("Lap time: " + GetLapTimeString());
    }
 
    public void OnFinishedRace()
    {
        StopCoroutine("RunTimer"); float totalSecondsBeforeThisLap = 0;
        for (int i = 0; i < lapTimesList.Count; i++)
        {
            totalSecondsBeforeThisLap += lapTimesList[i];
        }

        lapTimesList.Add(seconds - totalSecondsBeforeThisLap);
        Debug.Log("Lap time: " + GetLapTimeString());

    }

    public string GetLapTimeString()
    {
        seconds = (int)(timeInSeconds % 60);

        minutes = (int)timeInSeconds / 60;
        milliseconds = (int)(timeInSeconds % 1 * 100);

        return GetFormattedTimeString(minutes, seconds, milliseconds);
    }

    public static string GetLapTimeString(float theseSeconds)
    {
        int seconds = (int)(theseSeconds % 60);

        int minutes = (int)theseSeconds / 60;
        int milliseconds = (int)(theseSeconds % 1 * 100);

        return GetFormattedTimeString(minutes, seconds, milliseconds);
    }

    public static string GetFormattedTimeString(float minutes, float seconds, float milliseconds)
    {
        return $"{minutes.ToString("00")}:{seconds.ToString("00")}.{milliseconds.ToString("00")}";
    }

    IEnumerator RunTimer()
    {
        while(isTimeRunning)
        {
            yield return new WaitForEndOfFrame();
            timeInSeconds += Time.deltaTime;
            // update text
            //Debug.Log(time);
            seconds = (int)(timeInSeconds % 60);

            if (seconds > 9999)
            {
                SceneChangeManager.Instance.LoadScene(0);
            }

            minutes = (int)timeInSeconds / 60;
            milliseconds = (int)(timeInSeconds % 1 * 100);

            RacingUIManager.Instance.SetTimerText(GetFormattedTimeString(minutes, seconds, milliseconds));
        }       
    }


}
