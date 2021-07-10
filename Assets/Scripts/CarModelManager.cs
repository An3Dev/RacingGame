using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarModelManager : MonoBehaviour
{
    public static CarModelManager Instance;

    //GameObject[] carPrefabs;
    Car[] carComponents;

    public int currentCarIndex = 0;
    [SerializeField] CarsList carsList;
    RaceInformation raceInformation;

    private void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(this);
        carComponents = new Car[carsList.carPrefabsList.Length];
        for(int i = 0; i < carsList.carPrefabsList.Length; i++)
        {
            carComponents[i] = carsList.carPrefabsList[i].GetComponent<Car>();
        }

        raceInformation = ReferenceManager.Instance.GetRaceInformation();
        currentCarIndex = raceInformation.selectedCarIndex;
    }

    public void SetCurrentIndex(int index)
    {
        currentCarIndex = index;
        raceInformation.selectedCarIndex = currentCarIndex;
    }

    public int GetCurrentIndex()
    {
        return currentCarIndex;
    }

    public GameObject[] GetCarPrefabs()
    {
        return carsList.carPrefabsList;
    }

    public GameObject GetCarByIndex(int index)
    {
        return carsList.carPrefabsList[index];
    }

    public GameObject GetCurrentCarGameObject()
    {
        return carsList.carPrefabsList[currentCarIndex];
    }

    public Car GetCurrentCar()
    {
        return carComponents[currentCarIndex];
    }

    public Car GetCar(int index)
    {
        return carComponents[index];
    }

    public string GetCurrentCarName()
    {
        return carsList.carPrefabsList[currentCarIndex].name;
    }
    public int GetCarNumber()
    {
        return carComponents[currentCarIndex].GetCarNumber();
    }

    public int GetCarNumber(int index)
    {
        return carComponents[currentCarIndex].GetCarNumber();
    }

    public string GetCarName(int index)
    {
        return carsList.carPrefabsList[index].name;
    }

    public int GetCarIndexByCarNumber(int carNumber)
    {
        for(int i = 0; i < carsList.carPrefabsList.Length; i++)
        {
            if (carComponents[i].GetCarNumber() == carNumber)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetCarListLength()
    {
        return carsList.carPrefabsList.Length;
    }
}
