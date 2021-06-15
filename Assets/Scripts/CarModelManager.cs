using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarModelManager : MonoBehaviour
{
    public static CarModelManager Instance;

    public GameObject[] carPrefabs;
    Car[] carComponents;

    public static int currentCarIndex = 0;

    private void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(this);
        carComponents = new Car[carPrefabs.Length];
        for(int i = 0; i < carPrefabs.Length; i++)
        {
            carComponents[i] = carPrefabs[i].GetComponent<Car>();
        }
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

    public GameObject GetCurrentCarGameObject()
    {
        return carPrefabs[currentCarIndex];
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
        return carPrefabs[currentCarIndex].name;
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
        return carPrefabs[index].name;
    }

    public int GetCarIndexByCarNumber(int carNumber)
    {
        for(int i = 0; i < carPrefabs.Length; i++)
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
        return carPrefabs.Length;
    }
}
