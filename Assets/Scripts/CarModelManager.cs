using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarModelManager : MonoBehaviour
{
    public static CarModelManager Instance;

    public GameObject[] carPrefabs;

    public static int currentCarIndex = 0;

    private void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(this);
    }

    public void SetCurrentIndex(int index)
    {
        currentCarIndex = index;
    }

    public int GetCurrentIndex()
    {
        return currentCarIndex;
    }

    public GameObject[] GetCarPrefabs()
    {
        return carPrefabs;
    }

    public GameObject GetCarByIndex(int index)
    {
        return carPrefabs[index];
    }

    public GameObject GetCurrentCar()
    {
        return carPrefabs[currentCarIndex];
    }

    public string GetCurrentCarName()
    {
        return carPrefabs[currentCarIndex].name;
    }

    public int GetCarListLength()
    {
        return carPrefabs.Length;
    }
}
