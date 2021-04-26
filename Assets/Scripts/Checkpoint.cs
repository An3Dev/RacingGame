using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    int index;
    bool wasPassed = false;
    private void Awake()
    {
        index = transform.GetSiblingIndex();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointManager.Instance.SetCheckpoint(index);
            wasPassed = true;
        }
    }

    public bool WasPassed()
    {
        return wasPassed;
    }

    public void ResetState()
    {
        wasPassed = false;
    }
}
