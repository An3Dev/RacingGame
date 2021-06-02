using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PlayFab;
using PlayFab.ClientModels;

public class SettingsUI : MonoBehaviour
{

    //public delegate void SettingChanges();
    public event Action<bool> OnUseMilesChanged;

    public static SettingsUI Instance;

    public Slider masterVolumeSlider, bgMusicVolumeSlider, sfxVolumeSlider, memeAudioVolumeSlider;
    public TextMeshProUGUI masterVolumeText, bgMusicVolumeText, sfxVolumeText, memeAudioVolumeText;


    public Toggle showFpsToggle, useMilesToggle;

    public TMP_InputField userNameInputField;
    public TextMeshProUGUI userNameErrorText, userNameText;


    const string masterVolumeKey = "MasterVolume", bgMusicKey = "BGMusicVolume", soundFXKey = "SoundFXVolume",
        memeAudioKey = "MemeAudioVolume", showFpsKey = "ShowFPS", useMilesKey = "UseMiles";
    AudioManager audioManager;

    public Transform canvas;

    float master, bg, sfx, meme;

    bool showFps = false;
    public bool useMiles { get; private set; }

    public bool ShowFps
    {
        get { return showFps; }
    }

    bool isOpen = false;
    public bool IsOpen
    {
        get { return isOpen; }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        } 
        else
        {
            Instance = this;
        }

        GetSettings();
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
        audioManager.SetAudioLevels(master, bg, sfx, meme);
        SetUIWithStoredValues();
        userNameText.text = PlayfabManager.Instance.GetUsername() != null ? PlayfabManager.Instance.GetUsername() : "NO USERNAME";
    }

    private void OnEnable()
    {
        userNameErrorText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // if escape is pressed, close this screen
        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Open(false);

        }
    }


    public void SetUsername(string username)
    {
        userNameText.text = username;
    }

    public void Open(bool open)
    {
        if (!open)
        {
            canvas.gameObject.SetActive(false);
            if (RacingUIManager.Instance != null)
            {
                RacingUIManager.Instance.OnUnpause();
            }
            SavePreferences();
        }
        else
        {
            canvas.gameObject.SetActive(true);
        }

        isOpen = open;
    }

    public void OnClickSubmitUsername()
    {
        if (userNameInputField.text.Length < 3)
        {
            Debug.Log("Your username must be longer than 2 characters and shorter than 21 characters.");
            userNameErrorText.text = "Your username must be longer than 2 characters and shorter than 21 characters.";
            userNameErrorText.gameObject.SetActive(true);

        }
        else
        {
            PlayfabManager.Instance.TryUpdatingUsername(userNameInputField.text);
        }
    }

    public void OnUserNameUpdateFailed(PlayFabError error)
    {
        userNameErrorText.gameObject.SetActive(true);
        userNameErrorText.text = "Username change failed. Error: " + error.GenerateErrorReport();
    }

    public void OnUserNameSuccessfullyUpdated(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Successfully changed username to " + result.DisplayName);
        userNameText.text =  result.DisplayName;
        userNameErrorText.gameObject.SetActive(false);
    }

    void SavePreferences()
    {
        PlayerPrefs.SetFloat(masterVolumeKey, master);
        PlayerPrefs.SetFloat(bgMusicKey, bg);
        PlayerPrefs.SetFloat(soundFXKey, sfx);
        PlayerPrefs.SetFloat(memeAudioKey, meme);
        PlayerPrefs.SetString(showFpsKey, showFps.ToString());
        PlayerPrefs.SetString(useMilesKey, useMiles.ToString());

    }

    #region UI events
    public void OnMasterSliderMoved(float value)
    {
        master = value;
        masterVolumeText.text = ((int)(value * 10)).ToString();
        audioManager.SetAudioLevels(master, bg, sfx, meme);
    }

    public void OnUseMilesPerHourClicked(bool on)
    {
        useMiles = on;
        // if the event is not null, then invoke it
        OnUseMilesChanged?.Invoke(on);
    }

    public void OnBGSliderMoved(float value)
    {
        bg = value;
        bgMusicVolumeText.text = ((int)(value * 10)).ToString();
        audioManager.SetAudioLevels(master, bg, sfx, meme);
    }

    public void OnSFXSliderMoved(float value)
    {
        sfx = value;
        sfxVolumeText.text = ((int)(value * 10)).ToString();
        audioManager.SetAudioLevels(master, bg, sfx, meme);
    }

    public void OnMemeSliderMoved(float value)
    {
        meme = value;
        memeAudioVolumeText.text = ((int)(value * 10)).ToString();
        audioManager.SetAudioLevels(master, bg, sfx, meme);
    }

    public void OnFPSToggleHit(bool on)
    {
        showFps = on;
    }

    #endregion

    void GetSettings()
    {
        master = PlayerPrefs.GetFloat(masterVolumeKey, 0.5f);
        bg = PlayerPrefs.GetFloat(bgMusicKey, 0.5f);
        sfx = PlayerPrefs.GetFloat(soundFXKey, 0.5f);
        meme = PlayerPrefs.GetFloat(memeAudioKey, 0.5f);

        showFps = bool.Parse(PlayerPrefs.GetString(showFpsKey, "false"));
        useMiles = bool.Parse(PlayerPrefs.GetString(useMilesKey, "false"));
    }


    void SetUIWithStoredValues()
    {
        masterVolumeSlider.SetValueWithoutNotify(master);
        masterVolumeText.text = ((int)(master * 10)).ToString();

        bgMusicVolumeSlider.SetValueWithoutNotify(bg);
        bgMusicVolumeText.text = ((int)(bg * 10)).ToString();

        sfxVolumeSlider.SetValueWithoutNotify(sfx);
        sfxVolumeText.text = ((int)(sfx * 10)).ToString();

        memeAudioVolumeSlider.SetValueWithoutNotify(meme);
        memeAudioVolumeText.text = ((int)(meme * 10)).ToString();

        showFpsToggle.isOn = showFps;
        useMilesToggle.isOn = useMiles;
    }
}
