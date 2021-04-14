using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoints : MonoBehaviour
{   

    public Transform GetDefaultCheckpoint()
    {
        return transform.GetChild(0);
    }
}
