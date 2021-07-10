using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class RoomList : MonoBehaviourPunCallbacks
{

    [SerializeField] GameObject roomSlotPrefab;
    [SerializeField] Transform slotsParent;

    List<RoomInfo> roomInfoList = new List<RoomInfo>();
    List<RoomSlot> slotsList = new List<RoomSlot>();

    //Dummy data List
    //private List<ContactInfo> _contactList = new List<ContactInfo>();

    private void Awake()
    {
        
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.JoinLobby();
    }

    void InitData()
    {
        
    }

    #region PUN Callbacks

    public override void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList)
    {
        Debug.Log("Room list update");
        roomInfoList.Clear();
        foreach(RoomSlot s in slotsList)
        {
            s.gameObject.SetActive(false);
        }

        for(int i = 0; i < roomList.Count; i++)
        {
            // don't show rooms that are closed or inivisible
            if (!roomList[i].IsOpen || !roomList[i].IsVisible)
            {
                break;
            }

            RoomInfo r = new RoomInfo(roomList[i].Name, roomList[i].PlayerCount, roomList[i].MaxPlayers);
            roomInfoList.Add(r);
            RoomSlot slot;
            if (slotsList.Count < roomList.Count || slotsList[i].gameObject.activeInHierarchy)
            {
                slot = Instantiate(roomSlotPrefab, slotsParent).GetComponent<RoomSlot>();
            } else
            {
                slot = slotsList[i];
            }
            slot.gameObject.SetActive(true);
             
            slot.ConfigureCell(r);
            slotsList.Add(slot);
        }
    }

    #endregion

    #region DATA-SOURCE

    /// <summary>
    /// Data source method. return the list length.
    /// </summary>
    //public int GetItemCount()
    //{
    //    return roomInfoList.Count;
    //}

    ///// <summary>
    ///// Called for a cell every time it is recycled
    ///// Implement this method to do the necessary cell configuration.
    ///// </summary>
    //public void SetCell(ICell cell, int index)
    //{
    //    //Casting to the implemented Cell
    //    var item = cell as RoomSlot;
    //    item.ConfigureCell(roomInfoList[index], index);
    //}

    #endregion
}
