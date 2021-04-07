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

    public void OnSteer(InputAction.CallbackContext context)
    {
        //Debug.Log(context.action.ToString() + ": " +context.ReadValue<Vector2>().ToString());
        carController.ReceiveSteeringInput(context.ReadValue<float>());
    }

    public void OnAirRoll(InputAction.CallbackContext context)
    {
        //Debug.Log(context.action.ToString() + ": " +context.ReadValue<Vector2>().ToString());
        carController.ReceiveAirRollInput(context.ReadValue<Vector2>());
    }

    public void OnGas(InputAction.CallbackContext context)
    {
        //Debug.Log(context.action.ToString() + ": " +context.ReadValue<float>().ToString());
        carController.ReceiveGasInput(context.ReadValue<float>());
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        //Debug.Log(context.action.ToString() + ": " +context.ReadValue<Vector2>().ToString());
        carController.ReceiveBrakeInput(context.ReadValue<float>());
    }

    public void OnDrift(InputAction.CallbackContext context)
    {
        carController.ReceiveDriftInput(context.ReadValue<float>() == 1 ? true : false);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        //Debug.Log(context.action.ToString() + ": " + context.ReadValue<Vector2>().ToString());
    }
}
