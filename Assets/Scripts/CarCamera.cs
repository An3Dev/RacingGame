using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarCamera : MonoBehaviour
{
    public Transform carTarget;
    public CarController carController;

    [SerializeField] Vector3 defaultDistanceFromCar = new Vector3(0, 0, -10);
    [SerializeField] Vector3 reverseDistanceFromCar = new Vector3(0, 0, 10);

    [SerializeField] Vector3 rotationOffset = new Vector3(30, 0, 0);

    [SerializeField] float positionStiffness = 1;
    [SerializeField] float rotationStiffness = 1;
    [SerializeField] float airRotationStiffness = 10;

    Rigidbody carRb;

    Vector3 desiredRot;
    Vector3 desiredGlobalPosition;

    bool isInDriftCam = false;

    float driftCamTimer = 0;
    [Tooltip("The lower the value, the faster the camera moves to regular position from the drift position. At 1, it takes the longest")]
    [Range(0.01f, 1000)]
    [SerializeField] float driftCamTransitionMultiplier = 0.5f;

    //bool isTransitioningCamera;

    Vector3 offsetOnDriftEnd;
    Vector3 offsetOnDriftStart;

    float currentPositionStiffness;
    public float stiffnessOnTransition = 100;

    public float driftCamExitDelay = 1f;
    float carSquareVelocity;

    bool transitioningToRegularCam = false;

    private void Awake()
    {
        carRb = carTarget.GetComponent<Rigidbody>();
        currentPositionStiffness = positionStiffness;
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

        carSquareVelocity = carController.GetWorldVelocity().sqrMagnitude;
        bool isCarDriftingAndMoving = carController.GetIsDrifting() && carSquareVelocity > 0.1f;
        // if car is drifting and is moving 
        if (isCarDriftingAndMoving)
        {
            // just started drifting
            if (!isInDriftCam)
            {
                offsetOnDriftStart = carTarget.InverseTransformPoint(transform.position);
                driftCamTimer = 0;
                isInDriftCam = true;
                // start timer
                driftCamTimer += Time.deltaTime;
                transitioningToRegularCam = false;

            }

            // make camera follow behind the direction of velocity;
            Vector3 velocity = carController.GetWorldVelocity();
            desiredGlobalPosition = carTarget.position + velocity.normalized * defaultDistanceFromCar.z;
            desiredGlobalPosition.y = carTarget.position.y + defaultDistanceFromCar.y;
        } else // if not drifting
        {
            // if just stopped drifting
            if (isInDriftCam)
            {
                // position of camera relative to car.
                offsetOnDriftEnd = carTarget.InverseTransformPoint(transform.position);

                driftCamTimer = 0;
                isInDriftCam = false;
                driftCamTimer += Time.deltaTime;
                transitioningToRegularCam = true;
            }

            if (transitioningToRegularCam)
            {
                float xVel = Mathf.Abs(carController.GetLocalVelocity().x);
                if (xVel > 0.05f)
                {
                    Vector3 velocity = carController.GetWorldVelocity();
                    float forwardOffset = Mathf.Clamp01(1 / xVel);
                    //Debug.Log("X: " + xVel + "  Forward: " + forwardOffset);

                    desiredGlobalPosition = carTarget.position + (velocity + carTarget.forward * forwardOffset * driftCamTransitionMultiplier).normalized * defaultDistanceFromCar.z;
                    desiredGlobalPosition.y = carTarget.position.y + defaultDistanceFromCar.y;
                }else
                {
                    //Debug.Log("Stop transitioning");
                    transitioningToRegularCam = false;
                }  
            } else
            {
                Vector3 point = carTarget.TransformPoint(defaultDistanceFromCar);
                if (carController.GetIsInAir())
                {
                    point.y = carTarget.position.y + defaultDistanceFromCar.y;
                }
                desiredGlobalPosition = point;
            }

            //if (driftCamTimer != 0 && driftCamTimer < driftCamExitDelay)
            //{
            //    driftCamTimer += Time.deltaTime;

            //    // keep using velocity movement to smoothly transition to regular cam
            //    Vector3 velocity = carController.GetWorldVelocity();
            //    desiredGlobalPosition = carTarget.position + velocity.normalized * defaultDistanceFromCar.z;
            //    desiredGlobalPosition.y = carTarget.position.y + defaultDistanceFromCar.y;
            //} else
            //{
            //    driftCamTimer = 0;
            //    Vector3 point = carTarget.TransformPoint(defaultDistanceFromCar);
            //    if (carController.GetIsInAir())
            //    {
            //        point.y = carTarget.position.y + defaultDistanceFromCar.y;
            //    }
            //    desiredGlobalPosition = point;
            //}      
        }

        //if timer is not done ticking
        //float timerFraction = driftCamTimer / driftCamTransitionTime;
        //if (timerFraction != 0 && timerFraction != 1)
        //{
        //    if (timerFraction < 0.5f)
        //    {
        //        currentPositionStiffness = Mathf.Lerp(positionStiffness, stiffnessOnTransition, timerFraction * 2);
        //    } else
        //    {
        //        currentPositionStiffness = Mathf.Lerp(stiffnessOnTransition, positionStiffness, (driftCamTimer - 0.5f) / driftCamTransitionTime * 2);
        //    }
        //    if (timerFraction >= 1)
        //    {
        //        isTransitioningCamera = false;
        //        driftCamTimer = 1;
        //    } else
        //    {
        //        driftCamTimer += Time.deltaTime;
        //        isTransitioningCamera = true;
        //    }
        //}

        //if timer is not done ticking
        //if (driftCamTimer / driftCamTransitionTime != 0 && driftCamTimer / driftCamTransitionTime != 1)
        //{
        //    DriftCameraTransition();
        //    //Debug.Log("Drift cam transition");
        //    if (!isTransitioningFromDrift)
        //    {
        //        isTransitioningFromDrift = true;
        //    }
        //}
        //else
        //{
        //    MoveCameraTowardCar();
        //    if (!isCarDriftingAndMoving && currentPositionStiffness != positionStiffness)
        //    {
        //        currentPositionStiffness -= Time.deltaTime * stiffnessAfterStopDrifting * (1 / positionStiffnessReturnTime);
        //        if (currentPositionStiffness <= positionStiffness)
        //        {
        //            currentPositionStiffness = positionStiffness;
        //        }
        //    }
        //    if (isTransitioningFromDrift)
        //    {
        //        isTransitioningFromDrift = false;
        //    }
        //}
        MoveCameraTowardCar();
        RotateCameraTowardCar();
    }

    void RotateCameraTowardCar()
    {
        Vector3 dirToTarget = (carTarget.position - transform.position).normalized;
        desiredRot = Quaternion.LookRotation(dirToTarget).eulerAngles;

        desiredRot = desiredRot + rotationOffset;
        float rotStiffness = carController.GetIsInAir() ? airRotationStiffness : rotationStiffness;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(desiredRot), Time.deltaTime * rotStiffness);
    }

    void DriftCameraTransition()
    {
        if (isInDriftCam) // if transitioning to a drift camera position
        {
            //Debug.DrawLine(carTarget.TransformVector(offsetOnDriftStart), carTarget.TransformVector(offsetOnDriftStart) + Vector3.up, Color.red, 1);
            //Debug.DrawLine(desiredGlobalPosition, desiredGlobalPosition + Vector3.up, Color.blue, 1);

            transform.position = Vector3.Slerp(carTarget.TransformPoint(offsetOnDriftStart), desiredGlobalPosition, driftCamTimer / driftCamTransitionMultiplier);
            driftCamTimer += Time.deltaTime;
            if (driftCamTimer >= driftCamTransitionMultiplier)
            {
                driftCamTimer = driftCamTransitionMultiplier;
            }
        }
        else // if transitioning to regular camera
        {

            //currentPositionStiffness -= Time.deltaTime * stiffnessAfterStopDrifting * positionStiffnessReturnTime;
            //if (currentPositionStiffness <= positionStiffness)
            //{
            //    currentPositionStiffness = positionStiffness;
            //}

            transform.position = Vector3.Slerp(carTarget.TransformPoint(offsetOnDriftEnd), desiredGlobalPosition/*Vector3.Lerp(transform.position, desiredGlobalPosition, Time.deltaTime * currentPositionStiffness * (carController.GetSpeed() / 30 + 1))*/, (driftCamTimer / driftCamTransitionMultiplier)/** (carController.GetSpeed() / 30 + 1) * positionLag*/);
            driftCamTimer += Time.deltaTime;
            if (driftCamTimer >= driftCamTransitionMultiplier)
            {
                driftCamTimer = driftCamTransitionMultiplier;
                // set position stiffness to high value so that the camera smoothly transitions to a position with lag
                currentPositionStiffness = stiffnessOnTransition;
            }
        }
    }

    void MoveCameraTowardCar()
    {
        transform.position = Vector3.Lerp(transform.position, desiredGlobalPosition, Time.deltaTime * currentPositionStiffness * (carController.GetSpeed() / 30 + 1));
    }
}
