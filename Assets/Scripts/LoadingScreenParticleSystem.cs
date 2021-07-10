using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenParticleSystem : MonoBehaviour
{
    public ParticleSystem ps;
    public AudioSource audioSource;
    public void PlayParticleSystem()
    {
        ps.Play();
    }

    public void Bruh()
    {
        audioSource.Play();
    }
}
