using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class RoomSlot : MonoBehaviour
{
    //UI
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI roomOccupancy;
    public Button joinButton;

    private RoomInfo roomInfo;

    public void OnClick()
    {
        RoomManager.TryJoinRoom(roomName.text);
    }

    public void ConfigureCell(RoomInfo roomInfo)
    {
        this.roomInfo = roomInfo;

        roomName.text = roomInfo.RoomName;
        roomOccupancy.text = roomInfo.NumPlayersInRoom + "/" + roomInfo.MaxPlayersInRoom;
    }
}
public class RoomInfo
{
    public string RoomName;
    public int NumPlayersInRoom;
    public int MaxPlayersInRoom;

    public RoomInfo(string name, int numPlayers, int maxPlayers)
    {
        RoomName = name;
        NumPlayersInRoom = numPlayers;
        MaxPlayersInRoom = maxPlayers;
    }
}

