using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ControllerUISelection : MonoBehaviour
{
    private void OnEnable()
    {
        Invoke("Select", 0.01f);       
    }

    void Select()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
