using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;

public class MultiplayerMenuUIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TextMeshProUGUI infoText, errorMessageText;

    [SerializeField] PanelContainer panelContainer;
    [SerializeField] GameObject header, roomButtonsContainer, createRoomPanel, roomListPanel, roomPanel;
    [SerializeField] MultiplayerRoom room;
    WaitForSeconds wait = new WaitForSeconds(0.05f);

    public static Action<string> OnRoomNameChanged;
    public static Action<string, int> OnCreateRoom;
    public static Action OnWantToViewRooms, OnWantToJoinRandomRoom;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] Slider numPlayersSlider;

    private void Awake()
    {
        header.SetActive(false);
        panelContainer.ShowPanel(infoText.gameObject);
        StartCoroutine(nameof(UpdateConnectionStatusText));
    }
    public override void OnEnable()
    {
        base.OnEnable();
        RoomManager.OnConnected += OnConnected;
        RoomManager.OnError += OnError;
        RoomManager.OnSuccessfullyCreatedRoom += OnSuccessfullyCreatedRoom;
        RoomManager.OnSuccessfullyJoinedRoom += OnSuccessfullyJoinedRoom;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        RoomManager.OnConnected -= OnConnected;
        RoomManager.OnError -= OnError;
        RoomManager.OnSuccessfullyCreatedRoom -= OnSuccessfullyCreatedRoom;
        RoomManager.OnSuccessfullyJoinedRoom -= OnSuccessfullyJoinedRoom;
    }

    #region OnClick
    public void OnClickCreateRoomButton()
    {
        if (String.IsNullOrWhiteSpace(roomNameInputField.text) || roomNameInputField.text.Length == 0)
        {
            return;
        }
        OnCreateRoom?.Invoke(roomNameInputField.text, (int)numPlayersSlider.value);
    }

    public void OnClickOpenCreateRoom(bool create)
    {
        panelContainer.ShowPanel(createRoomPanel);
        errorMessageText.gameObject.SetActive(false);
        //roomButtonsContainer.SetActive(false);
        //createRoomPanel.SetActive(true);
    }

    public void OnClickViewRoomList()
    {
        OnWantToViewRooms?.Invoke();
        panelContainer.ShowPanel(roomListPanel);
        //roomListPanel.SetActive(true);
    }

    public void OnClickJoinRandomRoom()
    {
        OnWantToJoinRandomRoom?.Invoke();
    }

    public void OnClickBackButton()
    {
        if (roomButtonsContainer.activeInHierarchy)
        {
            PhotonNetwork.Disconnect();
            // load main menu
            SceneManager.LoadScene(0);
        }
        else if (roomPanel.activeInHierarchy)
        {
            PhotonNetwork.LeaveRoom();
            panelContainer.ShowPanel(roomButtonsContainer);
        }
        else
        {
            //roomButtonsContainer.SetActive(true);
            panelContainer.ShowPanel(roomButtonsContainer);
        }
    }
    #endregion

    public void OnSuccessfullyCreatedRoom()
    {
        infoText.gameObject.SetActive(true);
        infoText.text = "Joining Room";
        panelContainer.ShowPanel(createRoomPanel);
    }

    public void OnSuccessfullyJoinedRoom(string roomName, Dictionary<int, Player> playersDict)
    {
        //roomPanel.SetActive(true);
        panelContainer.ShowPanel(roomPanel);

        room.SetRoomName(roomName);
        room.SetPlayerDict(playersDict);

        infoText.gameObject.SetActive(false);
        errorMessageText.gameObject.SetActive(false);
    }

    void OnError(string message)
    {
        errorMessageText.gameObject.SetActive(true);
        errorMessageText.text = message;
        Invoke(nameof(DisableErrorText), 20);
    }

    void DisableErrorText()
    {
        errorMessageText.gameObject.SetActive(false);
    }

    public new void OnConnected()
    {
        panelContainer.ShowPanel(roomButtonsContainer);
        //roomButtonsContainer.SetActive(true);
        infoText.gameObject.SetActive(false);
        header.SetActive(true);
    }
  
    IEnumerator UpdateConnectionStatusText()
    {
        while(infoText.gameObject.activeInHierarchy)
        {
            infoText.text = PhotonNetwork.NetworkClientState.ToString();
            yield return wait;
        }
        StopCoroutine(nameof(UpdateConnectionStatusText));
    }
}
