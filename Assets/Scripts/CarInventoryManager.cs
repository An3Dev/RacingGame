using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInventoryManager : MonoBehaviour
{
    public static CarInventoryManager Instance;
    const string carInventoryKey = "CarInventoryKey";

    string unlockedCarsString;

    List<string> cars = new List<string>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        } else
        {
            Instance = this;
        }
        //PlayerPrefs.DeleteKey(carInventoryKey);
    }

    private void Start()
    {
        unlockedCarsString = PlayerPrefs.GetString(carInventoryKey, "0");
        char[] separators = new char[] { ',' };
        string[] tempArray = unlockedCarsString.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);

        // copy array to the list
        for(int i = 0; i < tempArray.Length; i++)
        {
            cars.Add(tempArray[i]);
        }
    }

    public void UnlockCar(int carNumber)
    {
        // don't unlock car if it's already unlocked
        if (IsCarUnlocked(carNumber))
        {
            return;
        }
        cars.Add(carNumber.ToString());
        string stringList = ListToString(cars);
        PlayerPrefs.SetString(carInventoryKey, stringList);
    }

    public string ListToString(List<string> list)
    {
        string s = "";
        for(int i = 0; i < list.Count; i++)
        {
            s += list[i] + ",";
        }

        // remove the last character
        s = s.Substring(0, s.Length - 1);

        return s;
    }

    public bool IsCarUnlocked(int carNumber)
    {
        for(int i = 0; i < cars.Count; i++)
        {
            if (int.Parse(cars[i]) == carNumber)
            {
                return true;
            }
        }
        return false;
    }

    public void TryUnlockCar(int carNumberOfCurrentCar)
    {
        int indexOfCurrentCar = CarModelManager.Instance.GetCarIndexByCarNumber(carNumberOfCurrentCar);

        // if the index is missing or if the index of the car is at the last position in the car list
        if (indexOfCurrentCar == -1 || indexOfCurrentCar == CarModelManager.Instance.GetCarListLength() - 1)
        {
            Debug.LogError("Car number is incorrect");
            return;
        }

        Car carToUnlock = CarModelManager.Instance.GetCar(indexOfCurrentCar + 1);
        if (carToUnlock == null)
            return;

        int carNumberToUnlock = carToUnlock.GetCarNumber();
        // if the car is already unlocked, don't try to unlock it again
        if (IsCarUnlocked(carNumberToUnlock))
        {
            return;
        }

        //Debug.Log("Unlocked " + CarModelManager.Instance.GetCarByIndex(indexOfCurrentCar + 1));
        UnlockCar(carNumberToUnlock);
        RacingUIManager.Instance.UnlockedCar(carNumberToUnlock);
    }
}
