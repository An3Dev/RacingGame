using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    public static ReferenceManager Instance;

    [SerializeField] RaceInformation raceInfo;
    [SerializeField] Camera mainCamera;

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

    public RaceInformation GetRaceInformation()
    {
        return raceInfo;
    }

    public Camera GetCamera()
    {
        return mainCamera;
    }
}
