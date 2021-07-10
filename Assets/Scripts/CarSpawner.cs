using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
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

        if (ReferenceManager.Instance.GetRaceInformation().mode == Mode.GhostRace && !PhotonNetwork.InRoom)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null);
        }

        Debug.Log(CarModelManager.Instance.GetCurrentCarGameObject());
        currentCar = PhotonNetwork.Instantiate("Cars/" + CarModelManager.Instance.GetCurrentCarGameObject().name, startingSpot.position, startingSpot.rotation);
        //currentCar = Instantiate(CarModelManager.Instance.GetCurrentCarGameObject());
        //currentCar.transform.position = startingSpot.position;
        //currentCar.transform.rotation = startingSpot.rotation;
    }  

    public GameObject GetCurrentCar()
    {
        return currentCar;
    }
}
