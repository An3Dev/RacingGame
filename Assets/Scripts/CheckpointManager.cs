using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    List<Checkpoint> checkpoints = new List<Checkpoint>();

    Checkpoint currentCheckpoint;

    int numCheckpointsReached = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }

        for(int i = 0; i < transform.childCount; i++)
        {
            checkpoints.Add(transform.GetChild(i).GetComponent<Checkpoint>());
        }
        SetCheckpoint(0);
    }

    public void SetCheckpoint(int index)
    {
        currentCheckpoint = checkpoints[index];
        if (index == checkpoints.Count - 1)
        {
            // user completed a lap
            if (checkpoints[(checkpoints.Count - 1) / 2].WasPassed() && checkpoints[checkpoints.Count - 2].WasPassed())
            {
                LapTimeManager.Instance.OnCompletedLap();
                ResetCheckpointStates();
            }
        }
    }

    public void ResetCheckpointStates()
    {
        for(int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].ResetState();
        }
    }

    public Transform GetMostRecentCheckpoint()
    {
        return currentCheckpoint.transform;
    }

    public Transform GetDefaultCheckpoint()
    {
        return transform.GetChild(0);
    }
}
