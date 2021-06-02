using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public static CarSpawner Instance;
    GameObject currentCar;
    public Transform startingSpot;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }
        Debug.Log(CarModelManager.Instance.GetCurrentCar());
        currentCar = Instantiate(CarModelManager.Instance.GetCurrentCar());
        currentCar.transform.position = startingSpot.position;
        currentCar.transform.rotation = startingSpot.rotation;
    }
    

    public GameObject GetCurrentCar()
    {
        return currentCar;
    }
}
