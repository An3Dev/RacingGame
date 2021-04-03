using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarInput : MonoBehaviour
{
    CarController carController;
    private void Awake()
    {
        carController = GetComponent<CarController>();    
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        //Debug.Log(context.action.ToString() + ": " +context.ReadValue<Vector2>().ToString());
        carController.ReceiveMovementInput(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        //Debug.Log(context.action.ToString() + ": " + context.ReadValue<Vector2>().ToString());
    }
}
