using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarInput : MonoBehaviour
{
    
    CarController carController;
    PlayerInput playerInput;

    bool inputEnabled = true;
    private void Awake()
    {
        carController = GetComponent<CarController>();
        playerInput = GetComponent<PlayerInput>();

        InputSystem.onDeviceChange += OnDeviceChanged;
    }

    public void EnableInput(bool enable)
    {
        inputEnabled = enable;
    }

    void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
                // New Device.
                //Debug.Log(device.displayName + " was connected.");
                break;
            case InputDeviceChange.Disconnected:
                // Device got unplugged.
                //Debug.Log(device.displayName + " was disconnected.");
                break;
            case InputDeviceChange.Reconnected:
                // Plugged back in.
                //Debug.Log(device.displayName + " was reconnected.");
                break;
            case InputDeviceChange.Removed:
                // Remove from Input System entirely; by default, Devices stay in the system once discovered.
                //Debug.Log(device.displayName + " was removed.");
                break;
            case InputDeviceChange.UsageChanged:
                ////Debug.Log(device.displayName + " usage changed.");
                break;
            case InputDeviceChange.ConfigurationChanged:
                //Debug.Log(device.displayName + " configuration changed.");
                break;
            default:
                // See InputDeviceChange reference for other event types.
                break;
        }
    }

    public void CheckCurrentInput(InputAction.CallbackContext context)
    {
        string displayName = context.control.device.displayName;
        if (displayName.Equals("Xbox Controller") || displayName.Equals("Switch Pro Controller") || displayName.Equals("PS4 Controller") || displayName.Equals("WebGL Gamepad"))
        {
            carController.SetIsControllerInput(true);
        }
        else
        {
            carController.SetIsControllerInput(false);
        }
    }

    public void OnResetCarPosition(InputAction.CallbackContext context)
    {
        if (!inputEnabled)
        {
            return;
        }
        CheckCurrentInput(context);
        carController.SpawnAtCheckpoint();
    }

    public void OnRestartGame(InputAction.CallbackContext context)
    {
        CheckCurrentInput(context);

        if (!inputEnabled)
        {
            return;
        }

        Debug.Log("Restart game");
        RaceManager.Instance.OnRestartGamePressed();
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        Debug.Log("Escaped from Chris the Dummy");
        CheckCurrentInput(context);
        // if in the main menu
        if (RacingUIManager.Instance == null)
            return;

        RacingUIManager.Instance.OnEscapeInputPressed();
        SettingsUI.Instance.OnEscapePressed();
    }

    public void OnSteer(InputAction.CallbackContext context)
    {
        CheckCurrentInput(context);

        carController.ReceiveSteeringInput(context.ReadValue<float>());
    }

    public void OnAirRoll(InputAction.CallbackContext context)
    {
        if (!inputEnabled)
        {
            return;
        }
        CheckCurrentInput(context);
        carController.ReceiveAirRollInput(context.ReadValue<float>());
    }

    public void OnAirRotation(InputAction.CallbackContext context)
    {
        if (!inputEnabled)
        {
            return;
        }
        CheckCurrentInput(context);
        carController.ReceiveAirRotInput(context.ReadValue<Vector2>());
    }

    public void OnDoAirRoll(InputAction.CallbackContext context)
    {
        if (!inputEnabled)
        {
            return;
        }
        CheckCurrentInput(context);
        carController.ReceiveDoAirRollInput(context.ReadValue<float>() == 1 ? true : false);
    }

    public void OnGas(InputAction.CallbackContext context)
    {
        if (!inputEnabled)
        {
            return;
        }
        CheckCurrentInput(context);
        carController.ReceiveGasInput(context.ReadValue<float>());
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        if (!inputEnabled)
        {
            return;
        }
        CheckCurrentInput(context);

        carController.ReceiveBrakeInput(context.ReadValue<float>());
    }

    public void OnDrift(InputAction.CallbackContext context)
    {
        if (!inputEnabled)
        {
            return;
        }
        CheckCurrentInput(context);

        carController.ReceiveDriftInput(context.ReadValue<float>() == 1 ? true : false);
    }

    //public void OnLook(InputAction.CallbackContext context)
    //{
    //    //Debug.Log(context.action.ToString() + ": " + context.ReadValue<Vector2>().ToString());
    //}

}
