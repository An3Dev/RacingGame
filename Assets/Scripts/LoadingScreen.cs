using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [SerializeField] RaceInformation raceInformation;
    [SerializeField] MapList mapList;
    public Slider progressBar;
    public TextMeshProUGUI progressText, clickToContinueText;

    public Animator carAnimator;

    public int buildIndexForSceneToLoad = 0;
    AsyncOperation sceneLoading;

    public bool didSetSceneIndex = false;

    bool finishedLoading = false;

    float progress = 0;

    bool openScene = false;

    float minLoadingTime = 2;

    private void Awake()
    {
        //Debug.Log(sceneToLoadBuildIndex);
        //if (didSetSceneIndex)
        //{

        buildIndexForSceneToLoad = An3.SceneUtility.SceneIndexFromName(mapList.list[raceInformation.selectedMapIndex]);
        StartCoroutine(LoadAsynchronously(buildIndexForSceneToLoad));
        //}

    }

    private void Update()
    {
        if (finishedLoading && ((Gamepad.current != null && Gamepad.current.buttonSouth.isPressed)) || Mouse.current.leftButton.isPressed)
        {
            openScene = true;
            //sceneLoading.allowSceneActivation = true;
        }
    }

    IEnumerator LoadAsynchronously(int buildIndex)
    {
        yield return new WaitForSeconds(0.1f);
        sceneLoading = SceneManager.LoadSceneAsync(buildIndex);

        // this doesn't allow unity to open the scene once it's loaded.       
        sceneLoading.allowSceneActivation = false;
        sceneLoading.completed += Test();
        progress = 0;

        float step = 0;
        float velocity = 0;
        while(!openScene)
        {
            // if the bar is visually at above 80% and the loading is completely done.
            if (progress >= 0.8f && sceneLoading.progress >= 0.9f)
            {
                carAnimator.enabled = true;
                clickToContinueText.gameObject.SetActive(true);
                progressText.text = ((int)(progressBar.value * 100)).ToString() + "%";
                yield return null;
                continue;
            }
            carAnimator.enabled = false;
            float fraction = (float)(sceneLoading.progress / 0.9f);
            progress = Mathf.Lerp(progress, fraction, step);
            if (progress > 0.8f)
            {
                progress = 0.8f;
            }

            Mathf.SmoothDamp(progress, fraction, ref velocity, minLoadingTime, 0.2f);
            progressBar.value = progress;
            progressText.text = (int)Mathf.Clamp(progress * 100, 0, 100f) + "%";

            step += Time.deltaTime / minLoadingTime;
            yield return new WaitForEndOfFrame();
        }

        // allow unity to open the scene
        sceneLoading.allowSceneActivation = true;
    }

    System.Action<UnityEngine.AsyncOperation> Test()
    {
        finishedLoading = true;
        didSetSceneIndex = false;
        return null;
    }
}
