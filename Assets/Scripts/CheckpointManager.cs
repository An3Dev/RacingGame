using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    List<Transform> checkpoints = new List<Transform>();

    Transform currentCheckpoint;
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
            checkpoints.Add(transform.GetChild(i));
        }
    }

    public void SetCheckpoint(int index)
    {
        currentCheckpoint = checkpoints[index];
    }

    public Transform GetMostRecentCheckpoint()
    {
        return currentCheckpoint;
    }

    public Transform GetDefaultCheckpoint()
    {
        return transform.GetChild(0);
    }
}
