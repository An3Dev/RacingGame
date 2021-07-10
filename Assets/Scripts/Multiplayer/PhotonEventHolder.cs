using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;

public class PhotonEventHolder : MonoBehaviour, IOnEventCallback
{
    public static Action<double> OnAllPlayersLoaded;

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == PhotonEvents.AllPlayersLoaded)
        {
            Debug.Log("Event raised");
            OnAllPlayersLoaded?.Invoke((double)photonEvent.CustomData);
        }
    }
}
