using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    int index;
    bool wasPassed = false;
    BoxCollider trigger;
    private void Awake()
    {
        index = transform.GetSiblingIndex();
        trigger = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointManager.Instance.SetCheckpoint(index);
            // disable trigger temporarily
            DisableCollider();
            Invoke(nameof(EnableCollider), 1f);

            wasPassed = true;
        }
    }

    void DisableCollider()
    {
        trigger.enabled = false;

    }
    void EnableCollider()
    {
        trigger.enabled = true;
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
