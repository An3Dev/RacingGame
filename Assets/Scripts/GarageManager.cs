using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class GarageManager : MonoBehaviour
{
    public static GarageManager Instance;

    public GameObject garageUI;
    public Transform spawnPosition;

    public GameObject leftArrowButton, rightArrowButton;
    public TextMeshProUGUI carNameText;

    public Slider speedSlider, brakeSlider, handlingSlider, stabilitySlider;

    int currentCarIndex = 0;

    GameObject car;

    const string currentCarIndexKey = "CurrentCarIndex";

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
    Dictionary<int, GameObject> spawnedCars = new Dictionary<int, GameObject>();

    private void OnEnable()
    {
        currentCarIndex = PlayerPrefs.GetInt(currentCarIndexKey, 0);
        CarModelManager.Instance.SetCurrentIndex(currentCarIndex);
        SpawnOrEnableCar();
        FillStats();
    }

    void SpawnOrEnableCar()
    {
        if (!spawnedCars.ContainsKey(currentCarIndex))
        {
            GameObject prefab = CarModelManager.Instance.GetCarByIndex(currentCarIndex);
            car = Instantiate(prefab);
            car.name = prefab.name;
            car.transform.position = spawnPosition.position;
            car.transform.rotation = spawnPosition.rotation;
            car.GetComponent<Car>().SetDrivable(false);
            spawnedCars.Add(currentCarIndex, car);
        } else
        {
            spawnedCars[currentCarIndex].SetActive(true);
            car = spawnedCars[currentCarIndex];
        }
    }

    public void OpenGarage(bool open)
    {
        garageUI.SetActive(open);
        MainMenu.Instance.ShowMainButtons(!open);
    }

    void DisableCurrentCar()
    {
        car.SetActive(false);
    }

    public void OnClickLeftArrow()
    {
        ChangeCar(-1);
    }

    public void OnClickRightArrow()
    {
        ChangeCar(1);
    }

    void ChangeCar(int dir)
    {
        DisableCurrentCar();

        currentCarIndex += dir;
        
        CheckIndexUnderOrOverflow();
        CarModelManager.Instance.SetCurrentIndex(currentCarIndex);
        PlayerPrefs.SetInt(currentCarIndexKey, currentCarIndex);
        UpdateShownCar();
    }

    public void CheckIndexUnderOrOverflow()
    {
        if (currentCarIndex > CarModelManager.Instance.GetCarListLength() - 1)
        {
            currentCarIndex = 0;
        }
        else if (currentCarIndex < 0)
        {
            currentCarIndex = CarModelManager.Instance.GetCarListLength() - 1;
        }
    }


    void UpdateShownCar()
    {
        // spawn or enable car
        //Debug.Log("Showing car " + CarModelManager.Instance.GetCarByIndex(currentCarIndex).name);
        SpawnOrEnableCar();

        // update the text with the car name
        FillStats();
    }

    void FillStats()
    {
        carNameText.text = car.name;

        CarSettings settings = car.GetComponent<Car>().GetCarSettings();
        speedSlider.value = settings.maxForwardVelocity / 100f;

        brakeSlider.value = (settings.brakeForce / 40f + settings.friction / 40f) / 2f;

        handlingSlider.value = settings.turnAmount / 120;

        stabilitySlider.value = settings.downForce / 80;
    }
}
