using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class GarageManager : MonoBehaviour
{
    public static GarageManager Instance;

    public RaceInformation raceInformation;
    public GameObject garageUI;
    public Transform spawnPosition;

    public GameObject leftArrowButton, rightArrowButton;
    public TextMeshProUGUI carNameText, unlockCarInstructionsTxt;

    public Slider speedSlider, brakeSlider, handlingSlider, stabilitySlider;

    int currentCarIndex = 0;

    GameObject car;

    int lastUnlockedCarIndex;

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
        raceInformation.selectedCarIndex = currentCarIndex;
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
        if (!open && currentCarIndex != lastUnlockedCarIndex)
        {
            ChangeCarToIndex(lastUnlockedCarIndex);
        }
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
        //CarModelManager.Instance.SetCurrentIndex(currentCarIndex);
        raceInformation.selectedCarIndex = currentCarIndex;

        if (CarInventoryManager.Instance.IsCarUnlocked(currentCarIndex))
        {
            Debug.Log("Car is unlocked");
            lastUnlockedCarIndex = currentCarIndex;
            PlayerPrefs.SetInt(currentCarIndexKey, currentCarIndex);
            unlockCarInstructionsTxt.gameObject.SetActive(false);

        }
        else
        {
            unlockCarInstructionsTxt.gameObject.SetActive(true);
            unlockCarInstructionsTxt.text = "To unlock this car, you have to beat the " + CarModelManager.Instance.GetCarName(currentCarIndex - 1) + " in Hard or Insane mode!";
            carNameText.text = "LOCKED";

            //Debug.Log("Car is not unlocked");

            //Debug.Log("To unlock the " + CarModelManager.Instance.GetCurrentCarName() + ", you have to beat the " + CarModelManager.Instance.GetCarName(currentCarIndex - 1) + " in Hard or Insane mode");
        }

        UpdateShownCar();
    }

    void ChangeCarToIndex(int index)
    {
        DisableCurrentCar();

        currentCarIndex = index;

        CheckIndexUnderOrOverflow();
        CarModelManager.Instance.SetCurrentIndex(currentCarIndex);
        raceInformation.selectedCarIndex = currentCarIndex;

        if (CarInventoryManager.Instance.IsCarUnlocked(currentCarIndex))
        {
            unlockCarInstructionsTxt.gameObject.SetActive(false);
            lastUnlockedCarIndex = currentCarIndex;
            PlayerPrefs.SetInt(currentCarIndexKey, currentCarIndex);
        }
        else
        {
            unlockCarInstructionsTxt.gameObject.SetActive(true);
            unlockCarInstructionsTxt.text = "To unlock this car, you have to beat the " + CarModelManager.Instance.GetCarName(currentCarIndex - 1) + " in Hard or Insane mode!";
            carNameText.text = "LOCKED";
            //Debug.Log("Car is not unlocked");

            //Debug.Log("To unlock the " + CarModelManager.Instance.GetCurrentCarName() + ", you have to beat the " + CarModelManager.Instance.GetCarName(currentCarIndex - 1) + " in Hard or Insane mode");
        }

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
