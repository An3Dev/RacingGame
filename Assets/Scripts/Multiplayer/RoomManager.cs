using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PlayFab;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static new Action OnConnected;
    public static Action<string> OnError;
    public static Action<string> TryJoinRoom;
    //public static Action<bool> OnClickCreateRoomAction, OnClickViewRoomListAction, OnJoinedRoomAction;
    public static Action OnSuccessfullyCreatedRoom;
    public static Action<string, Dictionary<int, Player>> OnSuccessfullyJoinedRoom;
    string roomName;
    string playerName;

    Dictionary<int, Player> playersInRoom;

    private void Start()
    {
        OnUserConnectedToPlayfab(true);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        MultiplayerMenuUIManager.OnRoomNameChanged += OnRoomNameChanged;
        MultiplayerMenuUIManager.OnCreateRoom += CreateRoom;
        MultiplayerMenuUIManager.OnWantToJoinRandomRoom += JoinRandomRoom;
        MultiplayerMenuUIManager.OnWantToViewRooms += ViewRooms;

        TryJoinRoom += JoinRoom;
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayfabManager.OnUserLoggedIn += OnUserConnectedToPlayfab;
        }
        else
        {
            OnUserConnectedToPlayfab(true);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        MultiplayerMenuUIManager.OnRoomNameChanged -= OnRoomNameChanged;
        MultiplayerMenuUIManager.OnCreateRoom -= CreateRoom;
        PlayfabManager.OnUserLoggedIn -= OnUserConnectedToPlayfab;
    }

    public void OnUserConnectedToPlayfab(bool boolean)
    {
        PhotonNetwork.AutomaticallySyncScene = false;

        playerName = PlayfabManager.Instance.GetUsername();
        if (String.IsNullOrEmpty(playerName))
        {
            playerName = "Player " + UnityEngine.Random.Range(0, 10000);
        }
        PhotonNetwork.NickName = playerName;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnRoomNameChanged(string name)
    {
        roomName = name;
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        OnConnected?.Invoke();
    }

    public override void OnJoinedLobby()
    {
        //Debug.Log("Entered lobby");
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
        //Debug.Log("Join random room");
    }

    public void ViewRooms()
    {
        //Debug.Log("View rooms");
    }

    public void JoinRoom(string roomName)
    {
        //Debug.Log("Try join room");
        PhotonNetwork.JoinRoom(roomName);
    }

    public void CreateRoom(string roomName, int maxPlayers)
    {
        byte players = (byte)maxPlayers;
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = players,
            EmptyRoomTtl = 10000,
            IsVisible = true,
            PublishUserId = true,
            IsOpen = true,           
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        //base.OnCreatedRoom();
        OnSuccessfullyCreatedRoom?.Invoke();
    }


    public override void OnJoinedRoom()
    {
        //base.OnJoinedRoom();
        //Debug.Log("Joined room");
        roomName = PhotonNetwork.CurrentRoom.Name;
        playersInRoom = PhotonNetwork.CurrentRoom.Players;


        OnSuccessfullyJoinedRoom?.Invoke(roomName, playersInRoom);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError(message);
        OnError?.Invoke("Failed to join a random room. Create a room instead!");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError(message);
        OnError?.Invoke("Failed to create room. " + message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    
}
