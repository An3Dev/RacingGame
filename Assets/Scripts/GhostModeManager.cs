using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { Noob, Easy, Medium, Hard, Insane }

public class GhostModeManager : MonoBehaviour
{
    public static GhostModeManager Instance;

    public static Difficulty currentDifficulty;
    private void Awake()
    {
        if (Instance == null)
        {
            //First run, set the instance
            Instance = this;
            //DontDestroyOnLoad(gameObject);

        }
        else if (Instance != this)
        {
            //Instance is not the same as the one we have, destroy old one, and reset to newest one
            Destroy(Instance.gameObject);
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    public void OnClickDifficultyButton(int difficulty)
    {
        currentDifficulty = (Difficulty)difficulty;
        MainMenu.Instance.StartGame();
    }
}
