using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    //public GameObject loadingScreenPrefab;

    GameObject currentLoadingScreen;
    //LoadingScreen loadingScreen;

    AsyncOperation sceneLoading;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        } else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void LoadScene(int index)
    {
        LoadingScreen.sceneToLoadBuildIndex = index;
        LoadingScreen.didSetSceneIndex = true;
        SceneManager.LoadScene("LoadingScreen");     
    }
    public void LoadScene(string name)
    {
        LoadScene(SceneManager.GetSceneByName(name).buildIndex);
    }
}
