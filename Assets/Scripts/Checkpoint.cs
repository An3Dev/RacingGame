using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    int index;
    bool wasPassed = false;
    BoxCollider trigger;
    public bool isFinishCheckpoint = false;
    private void Awake()
    {
        index = transform.GetSiblingIndex();
        trigger = GetComponent<BoxCollider>();
    }

    public bool GetIsFinishCheckpoint()
    {
        return isFinishCheckpoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!RaceManager.Instance.startedRace) return;

        if (other.transform.root.CompareTag("Player"))
        {          
            wasPassed = true;
            
            // disable trigger temporarily
            DisableCollider();
            //Invoke(nameof(EnableCollider), 1f);
            CheckpointManager.Instance.SetCheckpoint(index);
        }
    }

    public void DisableTriggerTemporarily()
    {
        trigger.enabled = false;
        Invoke(nameof(EnableCollider), 3);
    }

    public void DisableCollider()
    {
        trigger.enabled = false;

    }
    public void EnableCollider()
    {
        trigger.enabled = true;
    }

    public int GetIndex()
    {
        return index;
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
