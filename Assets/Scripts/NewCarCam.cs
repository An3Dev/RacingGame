using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class NewCarCam : MonoBehaviour
{
    Transform car;
    CarController carController;
    Car carScript;
   
    [SerializeField] float camMoveTime = 0.1f;
    [SerializeField] float minCamMoveTime = 0.05f;
    [SerializeField] AnimationCurve distanceFromCarBasedOnSpeed;

    [Tooltip("How stiff the camera rotation is. A value of 1 will always have the car at the center of the screen + offset")]
    [SerializeField] [Range(0.01f, 1)] float rotationStiffness;


    [SerializeField] Vector3 defaultOffsetFromCar = new Vector3(0, 3, -7);
    [SerializeField] Vector3 reverseOffsetFromCar = new Vector3(0, 3, -15);


    [SerializeField] float maxDistanceFromCar = 10;
    [SerializeField] float maxReverseDistanceFromCar = 20;
    [SerializeField] Vector3 lookOffset = new Vector3(25, 0, 0);

    float smoothTeleportDistance = 30;

    float carSpeed = 0;
    float carMaxSpeed;

    Vector3 cameraVelocity;

    bool smoothTeleport = false;

    bool carCloseToGround = false;

    //enum CamState { Regular, Air, Drift};

    //CamState camState = CamState.Regular;

    float currentMaxDistance;

    public ParticleSystem breeze;
    float breezePercent;

    private void Start()
    {
        car = CarSpawner.Instance.GetCurrentCar().transform;
        carController = car.GetComponent<CarController>();
        carMaxSpeed = carController.GetMaxSpeed();
        currentMaxDistance = maxDistanceFromCar;
        carScript = car.GetComponent<Car>();
        SetVariables();
    }

    void SetVariables()
    {
        camMoveTime = carScript.camMoveTime;
        minCamMoveTime = carScript.minCamMoveTime;
        distanceFromCarBasedOnSpeed = carScript.distanceFromCarBasedOnSpeed;
        rotationStiffness = carScript.rotationStiffness;
        defaultOffsetFromCar = carScript.defaultOffsetFromCar;
        reverseOffsetFromCar = carScript.reverseOffsetFromCar;
        maxDistanceFromCar = carScript.maxDistanceFromCar;
        maxReverseDistanceFromCar = carScript.maxReverseDistanceFromCar;
        lookOffset = carScript.lookOffset;
        breezePercent = carScript.breezePercent;
    }

    private void LateUpdate()
    {
        carSpeed = carController.GetSpeed();

        if (carSpeed / carController.GetMaxSpeed() > breezePercent)
        {
            breeze.Play();
        } else if (breeze.isPlaying)
        {
            breeze.Stop();
        }

        Vector3 desiredCamPos = Vector3.zero;

        float cameraAngleFromCarForward = Vector3.Angle(car.forward, transform.forward);
        // if moving backward
        if (!carController.GetIsDrifting() && cameraAngleFromCarForward < 25 && carController.GetRelativeDirection().z < 0)
        {
            desiredCamPos = car.TransformPoint(reverseOffsetFromCar);
            currentMaxDistance = maxReverseDistanceFromCar;
        } else // if moving forward
        {
            desiredCamPos = car.TransformPoint(defaultOffsetFromCar);
            currentMaxDistance = maxDistanceFromCar;
        }

        carCloseToGround = carController.GetIsCloseToGround();

        Vector3 newPos = Vector3.SmoothDamp(transform.position, desiredCamPos, ref cameraVelocity, Mathf.Lerp(minCamMoveTime, camMoveTime, distanceFromCarBasedOnSpeed.Evaluate(carSpeed / carMaxSpeed)));
        float distanceFromCar = Vector3.Distance(car.position, newPos);

        // if camera is slightly further away than the max distance from car
        if (!smoothTeleport && distanceFromCar > currentMaxDistance && distanceFromCar < smoothTeleportDistance)
        {
            // this is the direction to the desired cam pos
            Vector3 dirFromCarToCam = -(car.position - newPos).normalized;

            // makes the direction vector be flat, so that it's at the same y position as the car.
            dirFromCarToCam.y = 0;

            // set the new pos to the car position plus the direction to desired pos with a magnitude of maxDistanceFromCar
            newPos = car.position + dirFromCarToCam * (currentMaxDistance);

            // increase the height of the new pos so that it matches the default offset from the car.
            newPos += Vector3.up * defaultOffsetFromCar.y;
        }
        else
        if (distanceFromCar >= smoothTeleportDistance)
        {
            smoothTeleport = true;
            //Debug.LogWarning("Distance: " + distanceFromCar + " smooth teleport");
        }

        if (smoothTeleport && transform.position.x - newPos.x < 0.1f)
        {
            smoothTeleport = false;
            //Debug.Log("No more teleport");
        }
        
        transform.position = newPos;

        Vector3 lookAtPos = car.position + lookOffset;
        Vector3 rotation = Quaternion.LookRotation(lookAtPos - transform.position).eulerAngles;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotation), rotationStiffness);
    }



}
