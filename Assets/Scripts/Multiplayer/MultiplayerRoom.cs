using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MultiplayerRoom : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] TextMeshProUGUI roomNameText;
    [SerializeField] GameObject playerListContainer;
    [SerializeField] GameObject playerPrefab;
    public RaceInformation raceInformation;

    [SerializeField] TextMeshProUGUI readyUpButtonText;
    [SerializeField] GameObject startGameButton;

    bool isPlayerReady = false;
    bool canStartGame = false;
    Player localPlayer;

    Dictionary<Player, PlayerSlot> playerSlotsDict = new Dictionary<Player, PlayerSlot>();

    string roomName;
    Dictionary<int, Player> playersDictionary = new Dictionary<int, Player>();
    List<Player> playersList = new List<Player>();
    GameObject emptySlot = null;

    public override void OnEnable()
    {
        base.OnEnable();

        PhotonNetwork.AddCallbackTarget(this);

        localPlayer = PhotonNetwork.LocalPlayer;
        Hashtable hashtable = new Hashtable { { PlayerCustomProperties.IsPlayerReady, false }, { PlayerCustomProperties.PlayerLoadedLevel, false } };
        localPlayer.SetCustomProperties(hashtable);
        startGameButton.SetActive(false);

        CheckCanStartGame();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public void SetRoomName(string name)
    {
        roomName = name;
        roomNameText.text = name;
    }

    public void SetPlayerDict(Dictionary<int, Player> players)
    {
        if (playersDictionary.Count > 0)
            playersDictionary.Clear();
        this.playersDictionary = players;
        PopulatePlayerList();
        SpawnAllPlayerSlots();      
    }

    void PopulatePlayerList()
    {
        foreach (KeyValuePair<int, Player> entry in playersDictionary)
        {
            playersList.Add(entry.Value);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        playersDictionary = PhotonNetwork.CurrentRoom.Players;
        SpawnPlayerSlot(newPlayer);
        playersList.Add(newPlayer);

        PhotonNetwork.CurrentRoom.IsOpen = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // removes the player that left from the list, and disables the slot(so that it can be potentially reused).
        for(int i = 0; i < playersList.Count; i++)
        {
            if (playersList[i] == otherPlayer)   
            {          
                playerSlotsDict[playersList[i]].gameObject.SetActive(false);
                emptySlot = playerSlotsDict[playersList[i]].gameObject;
                playersList.Remove(playersList[i]);
                playerSlotsDict.Remove(otherPlayer);
            }
        }

        PhotonNetwork.CurrentRoom.IsOpen = true;
    }

    void CheckCanStartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (playersList.Count > 1)
            {
                bool allPlayersReady = true;
                // if all players are ready
                foreach(KeyValuePair<Player, PlayerSlot> slot in playerSlotsDict)
                {
                    if (!slot.Value.GetIsReady())
                    {
                        allPlayersReady = false;
                        break;
                    }
                }
                canStartGame = allPlayersReady;
            }
            else
            {
                canStartGame = false;
            }
            startGameButton.SetActive(canStartGame);
        }
    }

    void SpawnAllPlayerSlots()
    {
        for (int i = 0; i < playersList.Count; i++)
        {
            SpawnPlayerSlot(playersList[i]);
        }
    }


    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // do this for all players on enable
        
        changedProps.TryGetValue(PlayerCustomProperties.IsPlayerReady, out object isReady);
        playerSlotsDict[targetPlayer].SetIsReady((bool)isReady);
        if (targetPlayer.Equals(localPlayer))
        {
            isPlayerReady = (bool)isReady;
        }
        CheckCanStartGame();
    }

    void SpawnPlayerSlot(Player newPlayer)
    {
        GameObject slot;
        if (emptySlot != null)
        {
            slot = emptySlot;
        } else
        {
           slot  = Instantiate(playerPrefab, playerListContainer.transform);
        }
        PlayerSlot playerSlot = slot.GetComponent<PlayerSlot>();
        playerSlot.SetPlayerName(newPlayer.NickName);

        // set the player slot to show if player is ready, we do this because players that join after other players have already readied up wouldn't know that they were ready.
        newPlayer.CustomProperties.TryGetValue(PlayerCustomProperties.IsPlayerReady, out object isReady);
        if (isReady != null)
        {
            playerSlot.SetIsReady((bool)isReady);
        }     

        playerSlotsDict.Add(newPlayer, playerSlot);
    }

    public void OnClickReady()
    {

        // toggle ready
        isPlayerReady = !isPlayerReady;

        readyUpButtonText.text = isPlayerReady ? "Unready" : "Ready Up";

        // set room properties to ready
        Hashtable hashtable = new Hashtable { { PlayerCustomProperties.IsPlayerReady, isPlayerReady } };
        localPlayer.SetCustomProperties(hashtable);
    }

    public void OnClickStartGame()
    {
        // set race information with map and car
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions()
        { Receivers = ReceiverGroup.All };

        string sceneNameToLoad = "Kangarousel";

        object[] content = new object[] { sceneNameToLoad };
        PhotonNetwork.RaiseEvent(PhotonEvents.LoadScene, content, raiseEventOptions, SendOptions.SendReliable);
        //SceneManager.LoadScene("LoadingScreen");             
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == PhotonEvents.LoadScene)
        {
            object[] data = (object[])photonEvent.CustomData;
            string sceneName = data[0].ToString();

            // in the future, set this to the selected car and map
            raceInformation.selectedCarIndex = 0;
            raceInformation.selectedMapIndex = 0;

            raceInformation.mode = Mode.Multiplayer;
            raceInformation.numLaps = 3;
            //LoadingScreen.sceneToLoadBuildIndex = An3.SceneUtility.sceneIndexFromName(sceneName);
            //LoadingScreen.didSetSceneIndex = true;
            SceneManager.LoadScene("LoadingScreen");
        }
    }
}
