using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace An3
{
    public class SceneUtility
    {
        public static string NameFromIndex(int BuildIndex)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(BuildIndex);
            int slash = path.LastIndexOf('/');
            string name = path.Substring(slash + 1);
            int dot = name.LastIndexOf('.');
            return name.Substring(0, dot);
        }

        public static int SceneIndexFromName(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                string testedScreen = NameFromIndex(i);
                //print("sceneIndexFromName: i: " + i + " sceneName = " + testedScreen);
                if (testedScreen == sceneName)
                    return i;
            }
            return -1;
        }
    }
}


