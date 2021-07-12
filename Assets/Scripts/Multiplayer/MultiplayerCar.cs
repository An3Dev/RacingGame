using UnityEngine;
using Photon.Pun;
public class MultiplayerCar : MonoBehaviourPun
{
    [SerializeField] Component[] componentsToDelete;

    private void Awake()
    {
        
        if (ReferenceManager.Instance.GetRaceInformation().mode == Mode.GhostRace)
        {
            Destroy(this);
        } else
        {
            if (!photonView.IsMine)
            {
                transform.gameObject.tag = "Default";
                CreateNameplate();
                DestroyUnnecessaryComponents();
            }
        }      
    }

    void CreateNameplate()
    {
        Nameplate nameplate = ((GameObject)Instantiate(Resources.Load("PlayerNameplate"))).GetComponent<Nameplate>();
        nameplate.Initialize(ReferenceManager.Instance.GetCamera().transform, transform, photonView.Owner.NickName);
    }

    private void DestroyUnnecessaryComponents()
    {
        foreach (Component c in componentsToDelete)
        {
            Destroy(c);
        }
    }
}
