using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public int carNumber;
    [Header("Car Settings")]
    public CarSettings carSettings;
    public SkinnedMeshRenderer carMesh;
    public Transform frontRightWheel;
    public Transform frontLeftWheel;
    public Transform backLeftWheel;
    public Transform backRightWheel;
    //public Transform wheelColliders;
    public Transform frontCollisionCheckTransform;

    public Transform frontRightWheelTurnParent;
    public Transform frontLeftWheelTurnParent;

    public Transform centerOfMassTransform;
    public Transform centerOfMassWhileInAirTransform;
    public Transform[] groundCheckList;

    public Material brakesOnMat, brakesOffMat;
    public int brakesMaterialIndex;

    [Space]
    [Header("Camera Settings")]
    public float camMoveTime = 0.1f;
    public float minCamMoveTime = 0.05f;
    public AnimationCurve distanceFromCarBasedOnSpeed;

    [Tooltip("How stiff the camera rotation is. A value of 1 will always have the car at the center of the screen + offset")]
    [Range(0.01f, 1)] public float rotationStiffness;


    public Vector3 defaultOffsetFromCar = new Vector3(0, 3, -7);
    public Vector3 reverseOffsetFromCar = new Vector3(0, 3, -15);


    public float maxDistanceFromCar = 10;
    public float maxReverseDistanceFromCar = 20;
    public Vector3 lookOffset = new Vector3(25, 0, 0);

    public float breezePercent = 0.8f;

    bool isDriveable = true;

    CarController carController;
    CarInput carInput;
    Rigidbody rb;

    bool isBrakeLightOn = true;

    private void Awake()
    {
        carController = GetComponent<CarController>();
        carInput = GetComponent<CarInput>();
        rb = GetComponent<Rigidbody>();
    }
    public int GetCarNumber()
    {
        return carNumber;
    }

    public void TurnOnBrakeLights()
    {
        if (isBrakeLightOn)
            return;

        Material[] mat = carMesh.materials;
        mat[brakesMaterialIndex] = brakesOnMat;
        carMesh.materials = mat;

        isBrakeLightOn = true;
    }

    public void TurnOffBrakeLights()
    {
        if (!isBrakeLightOn) return;
        Material[] mat = carMesh.materials;
        mat[brakesMaterialIndex] = brakesOffMat;
        carMesh.materials = mat;

        isBrakeLightOn = false;
    }

    public CarSettings GetCarSettings()
    {
        return carSettings;
    }

    public void SetDrivable(bool isDriveable)
    {
        this.isDriveable = isDriveable;
        carController.enabled = isDriveable;
        carInput.EnableInput(isDriveable);
        carInput.enabled = isDriveable;

        rb.isKinematic = !isDriveable;
        rb.collisionDetectionMode = isDriveable ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.ContinuousSpeculative;
    }
}
