using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum Mode { GhostRace, Multiplayer}

//[CreateAssetMenu(fileName = "RaceInformation", menuName = "ScriptableObjects")]
public class RaceInformation : ScriptableObject
{
    public int selectedCarIndex;
    public int selectedMapIndex;
    public Mode mode;
    public Difficulty ghostDifficulty;
    public int numLaps = 1;

    public object[] scenes;

    //public static string GetSelectedMapName()
    //{
    //    return An3.SceneUtility.sceneIndexFromName("Kangarousel");
    //}
}


