using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapTimeManager : MonoBehaviour
{
    public static LapTimeManager Instance;
    bool isTimeRunning = false;

    float timeInSeconds = 0;
    float numLaps = 0;


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
    }

    // Start is called before the first frame update
    void Start()
    {
        StartTimer();
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

    public void StopTimer()
    {
        isTimeRunning = false;
        StopCoroutine("RunTimer");
    }

    public void OnCompletedLap()
    {
        Debug.Log("Completed lap in: " + timeInSeconds);
        numLaps++;
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

            if (seconds >= 59)
            {
            }

            minutes = (int)timeInSeconds / 60;
            milliseconds = (int)(timeInSeconds % 1 * 100);

            //Debug.Log("B4: " + milliseconds);
            //if (milliseconds >= 100)
            //{
            //    milliseconds /= 10;
            //}
            //Debug.Log("After: " + milliseconds);
            //string m = milliseconds.ToString("00");

            RacingUIManager.Instance.SetTimerText($"{minutes.ToString("0")}:{seconds.ToString("00")}.{milliseconds.ToString("00")}");
        }       
    }


}
