using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
public class MultiplayerCar : MonoBehaviourPun
{
    [SerializeField] Component[] componentsToDelete;

    bool hasNameplate = false;
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name.Equals("LoadingScreen"))
        {
            Debug.Log("Destroyed game object");
            gameObject.SetActive(false);
            return;
        }

        if (ReferenceManager.Instance.GetRaceInformation().mode == Mode.GhostRace)
        {
            Destroy(this);
        } else
        {
            if (!photonView.IsMine)
            {
                Player[] players = PhotonNetwork.PlayerListOthers;
                for(int i = 0; i < players.Length; i++)
                {
                    if (players[i].ActorNumber == photonView.OwnerActorNr)
                    {
                        Nameplate nameplate = ((GameObject)Instantiate(Resources.Load("PlayerNameplate"))).GetComponent<Nameplate>();
                        
                        nameplate.SetText(players[i].NickName);
                        nameplate.SetFollowTarget(transform);
                        Debug.Log("Multiplayer Car" + ReferenceManager.Instance.GetCamera());
                        nameplate.SetLookTarget(ReferenceManager.Instance.GetCamera().transform);
                        hasNameplate = true;
                        break;
                    }
                }

                foreach (Component c in componentsToDelete)
                {
                    Destroy(c);
                }
            }            
        }      
    }

    private void OnEnable()
    {
        if (!hasNameplate)
        {
            if(!photonView.IsMine)
            {
                Player[] players = PhotonNetwork.PlayerListOthers;
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].ActorNumber == photonView.OwnerActorNr)
                    {
                        Nameplate nameplate = ((GameObject)Instantiate(Resources.Load("PlayerNameplate"))).GetComponent<Nameplate>();

                        nameplate.SetText(players[i].NickName);
                        nameplate.SetFollowTarget(transform);
                        Debug.Log("Multiplayer Car" + ReferenceManager.Instance.GetCamera());
                        nameplate.SetLookTarget(ReferenceManager.Instance.GetCamera().transform);
                        hasNameplate = true;
                        break;
                    }
                }

                foreach (Component c in componentsToDelete)
                {
                    Destroy(c);
                }
            }
        }
    }
}
