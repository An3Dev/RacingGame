using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    AudioMixer mixer;
    public AudioMixerGroup masterAudioMixer, bgAudioMixer, sfxAudioMixer, memeAudioMixer;

    float masterVolume = 0.5f, bgVolume = 0.5f, sfxVolume = 0.5f, memeVolume = 0.5f;

    float minVolumeLevel = -20;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }
    }

    #region getters
    public float GetMaster()
    {
        return masterVolume;
    }

    public float GetBGVolume()
    {
        return bgVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public float GetMemeVolume()
    {
        return memeVolume;
    }
    #endregion

    public void SetAudioLevels(float master, float bg, float sfx, float meme)
    {
        masterVolume = master;
        bgVolume = bg;
        sfxVolume = sfx;
        memeVolume = meme;

        SetAudioMixer();
    }

    void SetAudioMixer()
    {
        masterAudioMixer.audioMixer.SetFloat("MasterVolume", masterVolume * -minVolumeLevel + minVolumeLevel);
        if (masterVolume == 0)
        {
            masterAudioMixer.audioMixer.SetFloat("MasterVolume", -80);
        }

        bgAudioMixer.audioMixer.SetFloat("BGVolume", bgVolume * -minVolumeLevel + minVolumeLevel);
        if (bgVolume == 0)
        {
            masterAudioMixer.audioMixer.SetFloat("BGVolume", -80);
        }

        sfxAudioMixer.audioMixer.SetFloat("SFXVolume", sfxVolume * -minVolumeLevel + minVolumeLevel);
        if (sfxVolume == 0)
        {
            masterAudioMixer.audioMixer.SetFloat("SFXVolume", -80);
        }

        memeAudioMixer.audioMixer.SetFloat("MemeVolume", memeVolume * -minVolumeLevel + minVolumeLevel);
        if (memeVolume == 0)
        {
            masterAudioMixer.audioMixer.SetFloat("MemeVolume", -80);
        }
    }
}
