using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapTimeManager : MonoBehaviour
{
    public static LapTimeManager Instance;
    bool isTimeRunning = false;

    float time = 0;
    float laps = 0;

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
        
    }

    public float GetTime()
    {
        return time;
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
        Debug.Log("Completed lap in: " + time);
    }

    IEnumerator RunTimer()
    {
        while(isTimeRunning)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
            // update text
            Debug.Log(time);
        }       
    }


}
