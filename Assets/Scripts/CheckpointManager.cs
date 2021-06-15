using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    public Transform checkpointContainer;
    public AudioSource checkpointAudioSource;

    List<Checkpoint> checkpoints = new List<Checkpoint>();

    Checkpoint currentCheckpoint;

    int checkpointsHit = 0;

    int nextCheckpoint = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }

        if (checkpoints.Count <= 0)
            PopulateCheckpointList();

        currentCheckpoint = checkpoints[checkpoints.Count - 1];
        nextCheckpoint = 0;


        //SetCheckpoint(0);
    }

    private void Start()
    {
        EnableAppropriateCheckpoint();
    }

    void PopulateCheckpointList()
    {
        for (int i = 0; i < checkpointContainer.childCount; i++)
        {
            checkpoints.Add(checkpointContainer.GetChild(i).GetComponent<Checkpoint>());
        }
    }

    public void EnableAppropriateCheckpoint()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (i == nextCheckpoint)
            {
                checkpoints[i].EnableCollider();
            } else
            {
                checkpoints[i].DisableCollider();
            }
        }
    }

    public void SetCheckpoint(int index)
    {
        // this is true if the current checkpoint was already passed
        checkpointsHit++;
        //Debug.Log("Checkpoints hit: " + checkpointsHit);

        currentCheckpoint = checkpoints[index];
        checkpointAudioSource.transform.position = checkpoints[index].transform.position;
        checkpointAudioSource.Play();

        nextCheckpoint++;
        if (nextCheckpoint > checkpoints.Count - 1)
        {
            nextCheckpoint = 0;
        }
        CheckCompletedLap(index);
        EnableAppropriateCheckpoint();
    }

    void CheckCompletedLap(int checkpointIndex)
    {
        if (checkpointIndex == checkpoints.Count - 1)
        {
            bool allCheckpointsWerePassed = true;
            for(int i = 0; i < checkpoints.Count; i++)
            {
                if (!checkpoints[i].WasPassed())
                {
                    allCheckpointsWerePassed = false;
                    break;
                }
            }

            // user completed a lap
            if (allCheckpointsWerePassed)
            {
                LapTimeManager.Instance.OnCompletedLap();
                RacingUIManager.Instance.OnCompletedLap();
                RaceManager.Instance.OnCompletedLap();
                GhostCapture.Instance.FinishedRace();
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

    public void OnResetCarPos()
    {
        foreach(Checkpoint c in checkpoints)
        {
            c.DisableTriggerTemporarily();
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
