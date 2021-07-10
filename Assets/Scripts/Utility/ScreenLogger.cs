using System.Collections;
using UnityEngine;

public class ScreenLogger : MonoBehaviour
{
    private static ScreenLogger Instance;
    private string log;
    private const int MAXCHARS = 10000;
    private Queue myLogQueue = new Queue();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        } else
        {
            DontDestroyOnLoad(this);
        }
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLogQueue.Enqueue("\n [" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue("\n" + stackTrace);
    }

    void Update()
    {
        while (myLogQueue.Count > 0)
            log = myLogQueue.Dequeue() + log;
        if (log.Length > MAXCHARS)
            log = log.Substring(0, MAXCHARS);
    }

    void OnGUI()
    {
        GUILayout.Label(log);
    }
}