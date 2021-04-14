using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarCamera : MonoBehaviour
{
    public Transform carTarget;
    public CarController carController;

    [SerializeField] Vector3 defaultDistanceFromCar = new Vector3(0, 0, -10);
    [SerializeField] Vector3 rotationOffset = new Vector3(30, 0, 0);

    [SerializeField] float positionLag = 1;
    [SerializeField] float rotationStiffness = 1;


    Rigidbody carRb;

    Vector3 desiredRot;
    Vector3 desiredPos;

    Vector3 offset;
    private void Awake()
    {
        carRb = carTarget.GetComponent<Rigidbody>();
        //offset = transform.position - carTarget.position;
        //defaultDistanceFromCar = offset;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
        {
            Time.timeScale -= 0.1f;
        }
        else if (Keyboard.current[Key.Digit2].wasPressedThisFrame)
        {
            Time.timeScale += 0.1f;
        }
        else if (Keyboard.current[Key.Digit3].wasPressedThisFrame)
        {
            Time.timeScale = 1;
        }
        Vector3 point = carTarget.TransformPoint(defaultDistanceFromCar);
        if (carController.GetIsInAir())
        {
            point.y = carTarget.position.y + defaultDistanceFromCar.y;
        }
        desiredPos = point;

        Vector3 dirToTarget = (carTarget.position - transform.position).normalized;
        desiredRot = Quaternion.LookRotation(dirToTarget).eulerAngles;

        desiredRot = desiredRot + rotationOffset;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(desiredRot), Time.deltaTime * rotationStiffness);
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * positionLag * (carController.GetSpeed() / 30 + 1));
    }
}
