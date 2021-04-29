using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Memes : MonoBehaviour
{
    public static Memes Instance;
    public AudioSource audioSource;

    AudioClip currentClip;

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

    public void SetClip(AudioClip clip)
    {
        currentClip = clip;
        audioSource.clip = currentClip;
    }

    public void PlayClipOneShot()
    {
        audioSource.PlayOneShot(currentClip);
    }

    
}
