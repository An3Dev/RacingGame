using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarInput))]

public class CarController : MonoBehaviour
{
    [Header("Car")]
    [SerializeField] CarSettings carSettings;

    [Space]

    [Header("Objects to Assign")]
    public CarCamera carCam;
    [SerializeField] SkinnedMeshRenderer carMesh;
    [SerializeField] Transform frontRightWheel;
    [SerializeField] Transform frontLeftWheel;
    [SerializeField] Transform backLeftWheel;
    [SerializeField] Transform backRightWheel;
    [SerializeField] Transform wheelColliders;



    [SerializeField] Transform frontRightWheelTurnParent;
    [SerializeField] Transform frontLeftWheelTurnParent;

    [SerializeField] Transform centerOfMassTransform;
    [SerializeField] Transform centerOfMassWhileInAirTransform;
    public CheckpointManager checkpoints;
    
    [Space]
    [Header("Car Properties")]
    [SerializeField] float rotForceWhenOnSide = 50;

    [SerializeField] Transform[] groundCheck;
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] int numWheelsOnGroundConsideredInAir = 2;
    [Tooltip("Angle between the car's forward direction and its velocity direction at which full acceleration is applied when going full speed")]
    [SerializeField] float fullAccelerationAngle = 91;

    [SerializeField] LayerMask groundMask;
    [SerializeField] float sideCheckDistance = 0.1f;


    float maxForwardVelocity = 10;
    float maxReverseVelocity = 10;   
    float accelerationMagnitude = 1;
    AnimationCurve accelerationOverSpeed;
    float reverseAccelerationMagnitude = 1;
    float tireSpinAmount = 360;

    float driftAcceleration = 10;
    AnimationCurve driftAccelerationOverSpeed;
    float driftFriction = 0.01f;
    float lateralDriftFriction = 0.01f;
    float driftingTurnAmount = 70;
    float speedWhereWheelsStartSpinningWhenDrifting = 5;

    float angleBelowNoDriftForceIsAdded = 10;

    
    float angleWhereFullDriftForceIsAdded = 20;
    float speedAtWhichDriftAccelerationStops = 20;


    float driftTireSpinAmount = 1440; // degrees per second
    float friction = 5;
    float lateralFriction = 20;

    float brakeForce = 30;
    float driftBrakeForce = 10;
    float drag = 0;
    float airDrag = 1;
    float angularDrag = 1;
    float postLandingAngularDrag = 1000;

    float turnAmount = 10;
    float speedAtWhichTurningSlows = 3;
    float maxTurnValue = 100;
    float turnSlowDownAmount = 10;
    float straightenOutSteeringWheelMultiplier = 1;
    bool isUsingController = false;

    float sideRotationForce = 70;
    float timeInAirBeforeCanAirRoll = 0.2f;
    float postLandingRotationLockTime = 0.2f;

    Rigidbody rb;


    float currentTurnValue = 0;
    bool wasInAirLastFrame = false;
    bool isInAir;
    Vector3 localVelocity = Vector3.zero;

    int numTiresOnGround;

    float timeSinceLeftAirTimer = 0;
    float postLandingTimer = -1;
    bool isDrifting = false;

    // input
    bool isDriftPressed = false;
    float gasInput, brakeInput;
    float steerInput;
    float airRollInput;
    Vector2 airRotInput;

    Vector3 worldDriftVelocity;

    bool airRollIsPressed = false;

    float startTime, endTime;

    float velocitySquareMagnitude;

    bool isOnSide = false;

    float groundCheckRate = 0.05f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassTransform.localPosition;

        SetVariablesFromScriptableObject();

        InvokeRepeating(nameof(GroundCheck), 0.00001f, groundCheckRate);
    }

    void SetVariablesFromScriptableObject()
    {
        maxForwardVelocity = carSettings.maxForwardVelocity;
        maxReverseVelocity = carSettings.maxReverseVelocity;
        accelerationMagnitude = carSettings.accelerationMagnitude;
        accelerationOverSpeed = carSettings.accelerationOverSpeed;
        reverseAccelerationMagnitude = carSettings.reverseAccelerationMagnitude;
        tireSpinAmount = carSettings.tireSpinAmount;

        driftAcceleration = carSettings.driftAcceleration;
        driftAccelerationOverSpeed = carSettings.driftAccelerationOverSpeed;
        driftFriction = carSettings.driftFriction;
        lateralDriftFriction = carSettings.lateralDriftFriction;
        driftingTurnAmount = carSettings.driftingTurnAmount;
        speedWhereWheelsStartSpinningWhenDrifting = carSettings.speedWhereWheelsStartSpinningWhenDrifting;
        angleBelowNoDriftForceIsAdded = carSettings.angleBelowNoDriftForceIsAdded;
        angleWhereFullDriftForceIsAdded = carSettings.angleWhereFullDriftForceIsAdded;
        speedAtWhichDriftAccelerationStops = carSettings.speedAtWhichDriftAccelerationStops;

        driftTireSpinAmount = carSettings.driftTireSpinAmount;

        friction = carSettings.friction;
        driftBrakeForce = carSettings.driftBrakeForce;
        lateralFriction = carSettings.lateralFriction;
        brakeForce = carSettings.brakeForce;
        drag = carSettings.drag;
        airDrag = carSettings.airDrag;
        angularDrag = carSettings.angularDrag;
        postLandingAngularDrag = carSettings.postLandingAngularDrag;

        turnAmount = carSettings.turnAmount;
        speedAtWhichTurningSlows = carSettings.speedAtWhichTurningSlows;
        maxTurnValue = carSettings.maxTurnValue;
        turnSlowDownAmount = carSettings.turnSlowDownAmount;
        straightenOutSteeringWheelMultiplier = carSettings.straightenOutSteeringWheelMultiplier;

        sideRotationForce = carSettings.sideRotationForce;

        rb.drag = drag;
        rb.angularDrag = angularDrag;
    }

    #region Receive Input
    public void ReceiveGasInput(float value)
    {
        gasInput = value;
    }

    public void ReceiveSteeringInput(float value)
    {
        steerInput = value;
    }

    public void SetIsControllerInput(bool isController)
    {
        isUsingController = isController;
    }

    public void ReceiveAirRollInput(float value)
    {
        airRollInput = value;
    }

    public void ReceiveAirRotInput(Vector2 value)
    {
        airRotInput = value;
    }

    public void ReceiveDoAirRollInput(bool doAirRoll)
    {
        this.airRollIsPressed = doAirRoll;
    }


    public void ReceiveBrakeInput(float value)
    {
        brakeInput = value;
    }

    public void ReceiveDriftInput(bool value)
    {
        isDriftPressed = value;
    }

    public void ResetCarPosition()
    {
        Transform t = checkpoints.GetMostRecentCheckpoint();
        transform.position = t.position;
        transform.rotation = t.rotation;
        rb.Sleep();
        rb.WakeUp();
    }

    #endregion

    void GroundCheck()
    {
        numTiresOnGround = 0;
        for (int i = 0; i < groundCheck.Length; i++)
        {
            Collider[] colliders = Physics.OverlapSphere(groundCheck[i].position, groundCheckRadius);
            foreach (Collider c in colliders)
            {
                if (!c.transform.root.Equals(transform))
                {
                    numTiresOnGround++;
                    break;
                }
            }
        }
    }

    void SideCheck()
    {
        int numTires = 0;
        for (int i = 0; i < groundCheck.Length; i++)
        {
            //Debug.DrawRay(groundCheck[i].position, groundCheck[i].forward * sideCheckDistance);
            if (Physics.Raycast(groundCheck[i].position, groundCheck[i].forward, sideCheckDistance, groundMask))
            {
                numTires++;
                if (numTires > 1)
                {
                    isOnSide = true;
                    return;
                }
            }
        }
        isOnSide = false;
    }

    private void Update()
    {
        //Debug.LogError("Error: Chris is a dummy.");
        // if car is in the air
        if (numTiresOnGround <= numWheelsOnGroundConsideredInAir)
        {   
            // if the car just became airborne.
            if (!wasInAirLastFrame)
            {
                rb.centerOfMass = centerOfMassWhileInAirTransform.localPosition;
                rb.drag = airDrag;
                localVelocity = Vector3.zero;
                InvokeRepeating(nameof(SideCheck), 0.0001f, groundCheckRate);
            }

            timeSinceLeftAirTimer += Time.deltaTime;
            wasInAirLastFrame = true;
            isInAir = true;
        }
        else // if the car is on the ground
        {
            isInAir = false;

            //if the car just landed
            if (wasInAirLastFrame)
            {
                // stop checking for side 
                CancelInvoke(nameof(SideCheck));

                //Debug.Log("Set velocity from air");
                localVelocity = transform.InverseTransformVector(rb.velocity);
                localVelocity.y = 0;
                wasInAirLastFrame = false;
                rb.centerOfMass = centerOfMassTransform.localPosition;
                rb.drag = drag;

                timeSinceLeftAirTimer = 0;
                postLandingTimer = 0;
            }
            // if it's time to unlock rotation
            if (postLandingTimer >= postLandingRotationLockTime)
            {
                rb.angularDrag = angularDrag;
                postLandingTimer = -1;
            }
            else if (postLandingTimer != -1)
            {
                rb.angularDrag = angularDrag + (postLandingTimer / postLandingRotationLockTime) * (postLandingAngularDrag - angularDrag);
                postLandingTimer += Time.deltaTime;
            }
        }

        if (!isInAir && isDriftPressed)
        {
            isDrifting = true;
            worldDriftVelocity = rb.velocity;
        }
        else
        {
            isDrifting = false;
        }

        Steering();
        SpinWheels();

        if (isInAir)
        {
            return;
        }

        //--------- This Code Does Not Execute While in the Air ------- //

        if (!isDrifting)
        {
            ApplyFriction();
        }
        else // is drifting
        {
            ApplyDriftFriction();
        }

        MoveCar();
        
        ApplyVelocityToRigidbody();
    }

    void MoveCar()
    {
        // if there is input
        if (gasInput != 0 || brakeInput != 0)
        {
            // if user wants to brake and we're not drifting.
            if (brakeInput != 0 && !isDrifting)
            {
                // braking
                if (localVelocity.z > 0 && brakeInput != 0)
                {
                    localVelocity.z -= brakeForce * brakeInput * Time.deltaTime;
                }
            }

            // if user wants to move forward
            if (gasInput > 0)
            {
                if (startTime == 0)
                {
                    startTime = Time.time;
                }

                // angle between direction in which the car wants to move forward, and the direction of velocity.
                float angle = Vector3.Angle(transform.forward, rb.velocity.normalized);

                if (!isDrifting)
                {
                    float accelerationMultiplier;
                    // if user wants to accelerate against the current velocity, apply full accleration force regardless of velocity;
                    if (angle > fullAccelerationAngle)
                    {
                        accelerationMultiplier = 1;
                        Debug.Log("Full acceleration");
                    } else
                    {
                        accelerationMultiplier = accelerationOverSpeed.Evaluate(localVelocity.z / maxForwardVelocity);
                        //Debug.Log("Acceleration from graph: " + accelerationMultiplier);

                    }
                    localVelocity += Vector3.forward * accelerationMagnitude * accelerationMultiplier * gasInput * accelerationMultiplier * Time.deltaTime;
                    // clamps the x and z axes
                    for(int i = 0; i < 3; i+= 2)
                    {
                        // clamps velocities of 
                        if (localVelocity[i] > maxForwardVelocity)
                        {
                            localVelocity[i] = maxForwardVelocity;
                            Debug.Log("Clamp " + i + "axis to max speed");
                        } else if (localVelocity[i] < -maxForwardVelocity)
                        {
                            localVelocity[i] = -maxForwardVelocity;
                            Debug.Log("Clamp " + i + "axis to negative max speed");

                        }
                    }
                }
                else
                {
                    AddWorldDriftVelocity(transform.forward, gasInput);
                }
            }
            // if user wants to go in reverse
            else if ((localVelocity.z <= 0.1f || isDrifting) && brakeInput > 0)
            {
                if (!isDrifting)
                {
                    localVelocity -= Vector3.forward * reverseAccelerationMagnitude * brakeInput * accelerationOverSpeed.Evaluate(-localVelocity.z / maxReverseVelocity) * Time.deltaTime;
                }
                else
                {
                    //if (transform.InverseTransformDirection(worldDriftVelocity).z > 0)
                    //{
                    //    worldDriftVelocity -= transform.TransformDirection(Vector3.forward * driftBrakeForce * Time.deltaTime);
                    //    //AddWorldDriftVelocity(-transform.forward, brakeInput);
                    //}
                }
            }
        }

        //Debug.DrawRay(transform.position + Vector3.up, rb.velocity.normalized * 5, Color.red, Time.deltaTime);
    }

    void AddWorldDriftVelocity(Vector3 forward, float input)
    {
        float angle = Vector3.Angle(forward, rb.velocity.normalized);

        velocitySquareMagnitude = rb.velocity.sqrMagnitude;
        //Debug.DrawRay(transform.position + Vector3.up, transform.forward * 5, Color.green, Time.deltaTime);

        // if car is moving slowly
        if (velocitySquareMagnitude < speedAtWhichDriftAccelerationStops * speedAtWhichDriftAccelerationStops)
        {
            //Debug.Log("Drift acceleration");
            Vector3 addition = forward * driftAcceleration * driftAccelerationOverSpeed.Evaluate(Mathf.Sqrt(velocitySquareMagnitude) / maxForwardVelocity) * input * Time.deltaTime;

            // if we apply the new velocity and the magnitude is not greater than the max forward velocity, the we actually apply the velocity.
            //if ((worldDriftVelocity + addition).sqrMagnitude < maxForwardVelocity * maxForwardVelocity)
            //{
                worldDriftVelocity += addition;
            //}
        }
        // if car is at an angle to add force against its velocity
        else if (angle > angleBelowNoDriftForceIsAdded)
        {

            float forceMultiplierBasedOnAngle = angle > angleWhereFullDriftForceIsAdded ? 1 : Mathf.Clamp01((angle - angleBelowNoDriftForceIsAdded) / (angleWhereFullDriftForceIsAdded - angleBelowNoDriftForceIsAdded));

            Vector3 addition = forward * forceMultiplierBasedOnAngle * driftAcceleration * driftAccelerationOverSpeed.Evaluate(Mathf.Sqrt(velocitySquareMagnitude) / maxForwardVelocity) * input * Time.deltaTime;
           
            // if we apply the new velocity and the magnitude is not greater than the max forward velocity, the we actually apply the velocity.
            if ((worldDriftVelocity + addition).sqrMagnitude < maxForwardVelocity * maxForwardVelocity)
            {
                //Debug.Log("Add drift force");
                worldDriftVelocity += addition;
            } else
            {
                //Debug.Log("Going too fast. Vector: " + worldDriftVelocity + " + " + addition + " = " + (worldDriftVelocity + addition).ToString("F3") +  ", Mag: " + (worldDriftVelocity + addition).magnitude);
            }
        } else
        {
            //Debug.Log("Angle: " + angle);
        }
    }

    void ApplyVelocityToRigidbody()
    {
        if (!isDrifting)
        {
            Vector3 worldVelocity = transform.TransformVector(localVelocity);
            worldVelocity.y = rb.velocity.y;
            rb.velocity = worldVelocity;
        } else
        {
            // use the world drift velocity that is set in the Drift method
            rb.velocity = worldDriftVelocity;
        }
    }

    void ApplyFriction()
    {
        worldDriftVelocity = Vector3.zero;
        // if there's no input, and the car is not moving downhill, slow down the car
        if (gasInput == 0 && localVelocity.z != 0 && rb.velocity.y > -1)
        {
            int slowDownDir = 1;
            if (localVelocity.z < 0)
            {
                slowDownDir = -1;
            }

            localVelocity.z -= slowDownDir * friction * Time.deltaTime;
            if (Mathf.Abs(localVelocity.z) < 0.01f)
            {
                localVelocity.z = 0;
            }
        }

        if (localVelocity.x != 0)
        {
            int slowDownDir = 1;
            if (localVelocity.x < 0)
            {
                slowDownDir = -1;
            }
            //float f = isDriftPressed ? driftFriction : sideFriction;
            localVelocity.x -= slowDownDir * lateralFriction * Time.deltaTime;
            if (Mathf.Abs(localVelocity.x) < 0.1f)
            {
                localVelocity.x = 0;
            }
        }
    }

    void ApplyDriftFriction()
    {
        Vector3 localDriftVelocity = transform.InverseTransformVector(worldDriftVelocity);
        Vector2 slowDownDir = Vector2.one;

        if (localDriftVelocity.x > 0)
        {
            slowDownDir.x = -1;
        }
        if (localDriftVelocity.z > 0)
        {
            slowDownDir.y = -1;
        }

        localDriftVelocity.x += slowDownDir.x * lateralDriftFriction * Time.deltaTime;
        localDriftVelocity.z += slowDownDir.y * driftFriction * Time.deltaTime;

        if (Mathf.Abs(localDriftVelocity.x) < 0.01f)
        {
            localDriftVelocity.x = 0;
        }
        if (Mathf.Abs(localDriftVelocity.z) < 0.01f)
        {
            localDriftVelocity.z = 0;
        }
        localVelocity = localDriftVelocity;
        worldDriftVelocity = transform.TransformVector(localDriftVelocity);
    }

    private void Steering()
    {
        int turnDir = 1;
        if (localVelocity.z < 0 && !isDrifting)
            turnDir = -1;

        float magnitude = rb.velocity.magnitude;
        float turningMultiplier = (magnitude > speedAtWhichTurningSlows) ? 1 : Mathf.Abs(magnitude / speedAtWhichTurningSlows);

        float turningValuePercent;

        if (!isUsingController)
        {
            // if player wants to turn left, but car is turning right, and vice versa

            if (steerInput != 0)
            {
                currentTurnValue += steerInput * Time.deltaTime * 100;
            }
            else
            {
                if (Mathf.Abs(currentTurnValue) > straightenOutSteeringWheelMultiplier * Time.deltaTime)
                {
                    float val = straightenOutSteeringWheelMultiplier * Time.deltaTime;
                    //val = Mathf.Clamp(val, 0, 0.999f);
                    currentTurnValue -= val * Mathf.Sign(currentTurnValue);
                }
                else
                {
                    currentTurnValue = 0;
                }
            }
            // clamp turn value
            if (currentTurnValue > maxTurnValue)
            {
                currentTurnValue = maxTurnValue;
            }
            else if (currentTurnValue < -maxTurnValue)
            {
                currentTurnValue = -maxTurnValue;
            }

            turningValuePercent = currentTurnValue / maxTurnValue;
        }
        // else if user is using analog steering
        else
        {
            turningValuePercent = steerInput;
        }

        float zRot = 30 * turningValuePercent;
        frontRightWheelTurnParent.localRotation = Quaternion.Euler(new Vector3(0, 0, zRot));
        //frontRightWheel.Rotate(Vector3.right * zRot, Space.Self);
        frontLeftWheelTurnParent.localRotation = Quaternion.Euler(new Vector3(0, 0, zRot));

        float thisTurnAmount = isDrifting ? driftingTurnAmount : turnAmount;
        if (!isInAir)
        {
            float slowDownDir = localVelocity.z < 0 ? -1 : 1;
            //Debug.Log("Slow down dir: " + slowDownDir);
            localVelocity.z -= turningMultiplier * slowDownDir * Mathf.Abs(turningValuePercent) * turnSlowDownAmount * Time.deltaTime;
            rb.MoveRotation(transform.rotation * Quaternion.AngleAxis(turningValuePercent * turnDir * thisTurnAmount * turningMultiplier * Time.deltaTime, transform.up));
        }
    }

    void SpinWheels()
    {
        Vector3 localRigidbodyVelocity = transform.InverseTransformVector(rb.velocity);
        if (!isInAir)
        {
            float frontWheelRotAmount = 0;
           

            // change this so that the back wheels rotate when drifting, but the front wheels rotate based on the velocity
            if (gasInput != 0)
            {
                if (!isDrifting)
                {
                    frontWheelRotAmount = localRigidbodyVelocity.z * tireSpinAmount * Time.deltaTime;
                }
                else
                {
                    frontWheelRotAmount = gasInput * driftTireSpinAmount * Time.deltaTime;
                }
            } 
            else if (brakeInput != 0)
            {
                if (!isDrifting)
                {
                    frontWheelRotAmount = -localRigidbodyVelocity.z * tireSpinAmount * Time.deltaTime;
                }
                else
                {
                    frontWheelRotAmount = -brakeInput * driftTireSpinAmount * Time.deltaTime;
                }
            } else if (localRigidbodyVelocity != Vector3.zero)
            {
                if (isDrifting)
                {
                    if (localRigidbodyVelocity.z > speedWhereWheelsStartSpinningWhenDrifting)
                    {
                        frontWheelRotAmount = (localRigidbodyVelocity.z - speedWhereWheelsStartSpinningWhenDrifting) * driftTireSpinAmount / 10 * Time.deltaTime;
                    }
                } else
                {
                    frontWheelRotAmount = localRigidbodyVelocity.z * tireSpinAmount * Time.deltaTime;
                }
            }

            if (frontWheelRotAmount != 0)
            {
                backLeftWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
                backRightWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
                frontRightWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
                frontLeftWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
            }
        } else if (!wasInAirLastFrame)
        {
            // store the wheel spin speed
        } else
        {
            // slow down the wheel spinning when in the air to a complete stop
        }
    }

    #region Getters
    public bool GetIsInAir()
    {
        return isInAir;
    }

    public float GetSpeed()
    {
        return rb.velocity.magnitude;
    }

    public Vector3 GetWorldVelocity()
    {
        return rb.velocity;
    }

    public Vector3 GetLocalVelocity()
    {
        return transform.InverseTransformVector(rb.velocity);
    }

    public bool GetIsDrifting()
    {
        return isDrifting;
    }

    #endregion

    private void FixedUpdate()
    {
        // if in the air
        if (numTiresOnGround <= numWheelsOnGroundConsideredInAir)
        {
            if (timeSinceLeftAirTimer > timeInAirBeforeCanAirRoll)
            {
                carMesh.SetBlendShapeWeight(0, Mathf.Clamp(carMesh.GetBlendShapeWeight(0) + Time.deltaTime * 300, 0, 100));
                carMesh.SetBlendShapeWeight(1, Mathf.Clamp(carMesh.GetBlendShapeWeight(1) + Time.deltaTime * 300, 0, 100));
                isInAir = true;

                float rotForce = isOnSide ? rotForceWhenOnSide : sideRotationForce;
                // if on keyboard and mouse, you can air roll without holding air roll button. On controller you have to hold air roll(drift) button to air roll.
                if ((!isUsingController || airRollIsPressed) && airRollInput != 0)
                {
                    rb.AddRelativeTorque(Vector3.forward * -airRollInput * rotForce, ForceMode.Acceleration);
                }

                if (airRotInput != Vector2.zero)
                {
                    rb.AddRelativeTorque(Vector3.right * airRotInput.y * rotForce, ForceMode.Acceleration);
                    rb.AddRelativeTorque(Vector3.up * airRotInput.x * rotForce, ForceMode.Acceleration);
                }
            }          
        } else
        {
            carMesh.SetBlendShapeWeight(0, Mathf.Clamp(carMesh.GetBlendShapeWeight(0) - Time.deltaTime * 200, 0, 100));
            carMesh.SetBlendShapeWeight(1, Mathf.Clamp(carMesh.GetBlendShapeWeight(1) - Time.deltaTime * 200, 0, 100));
        }
    }
}
