using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemeAudio : MonoBehaviour
{
    [SerializeField] AudioClip clipToPlayOnTrigger;
    bool entered = false;
    private void OnTriggerEnter(Collider other)
    {
        if (entered || !other.CompareTag("Player"))
            return;
        entered = true;
        Memes.Instance.SetClip(clipToPlayOnTrigger);
        Memes.Instance.PlayClip();
    }

    private void OnTriggerExit(Collider other)
    {
        entered = false;
    }
}
