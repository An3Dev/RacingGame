using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class CarController : MonoBehaviour
{
    #region Variables

    Car car;
    CarSettings carSettings;
    CarInput carInput;

    [Header("Objects to Assign")]
    SkinnedMeshRenderer carMesh;
    Transform frontRightWheel;
    Transform frontLeftWheel;
    Transform backLeftWheel;
    Transform backRightWheel;
// Transform wheelColliders;
    Transform frontCollisionCheckTransform;

    Transform frontRightWheelTurnParent;
    Transform frontLeftWheelTurnParent;

    Transform centerOfMassTransform;
    Transform centerOfMassWhileInAirTransform;
    Transform[] groundCheckList;
    [Tooltip("The time it takes for the car to go to the air state after it has detected that it is off the ground. This makes the experience smoother on little bumps where you're in the air for a millisecond but you can feel that you lose control.")]
    //float airDelay = 0;
    CheckpointManager checkpoints;
    
    [Space]
    [Header("Car Properties")]
    float maxTimeOnOffLimitArea = 1;

    float rotForceWhenOnSide = 30;


    float groundCheckDistance = 0.12f;
    int numWheelsOnGroundConsideredInAir = 2;
    [Tooltip("Angle between the car's forward direction and its velocity direction at which full acceleration is applied when going full speed")]
    float fullAccelerationAngle = 135;

    [SerializeField] LayerMask groundMask;
    float sideCheckDistance = 0.5f;


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

    [Tooltip("If the angle between the car forward direction and the car velocity direction is less than this value, then no force will be added at all.")]
    float angleBelowNoDriftForceIsAdded = 10;

    [Tooltip("If the angle between the car forward direction and the car velocity direction is greater than or equal to this value, then full drift force will be added." +
    " If the angle is less than this value, then a fraction of the full drift force will be added. If the angle is equal to the angleBelowNoDriftForceIsAdded value, then the smallest amount of drift force will be added. " +
    " If the angle is halfway between the angleBelowNoDriftForceIsAdded value and between this value, the half of the full drift force is added. ")]
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
    float fullSteerTime = 0.5f;
    float turnSlowDownAmount = 10;
    //float straightenOutSteeringWheelMultiplier = 1;
    float straightenOutWheelTime = 0.1f;
    bool isUsingController = false;

    float sideRotationForce = 70;
    float timeInAirBeforeCanAirRoll = 0.2f;
    //float postLandingRotationLockTime = 0.2f;

    Rigidbody rb;


    float downForce;
    float currentTurnValue = 0;
    bool wasInAirLastFrame = false;
    bool isInAir;
    Vector3 localVelocity = Vector3.zero;

    int numTiresOnGround;

    float timeSinceLeftAirTimer = 0;
    //float postLandingTimer = -1;
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

    bool isCloseToGround = false;
    bool isBlockedInFront = false;

    //bool stayOnSurface = false;

    float averageDistanceFromGround;

    float currentTimeOnOffLimitArea = 0;

    // first value is front left, then front right, back left, back right
    private float[] tireDistances = new float[4];
    #endregion
    private void Awake()
    {
        car = GetComponent<Car>();
        carSettings = car.GetCarSettings();

        rb = GetComponent<Rigidbody>();   

        SetVariablesFromCarSettings();
        rb.centerOfMass = centerOfMassTransform.localPosition;
        carInput = GetComponent<CarInput>();
    }

    private void Start()
    {
        checkpoints = CheckpointManager.Instance;
        
        // invoke repeating will call a function forever at a certain rate.
        InvokeRepeating(nameof(GroundCheck), 0.00001f, groundCheckRate);
        InvokeRepeating(nameof(FrontCollisionCheck), 0.00001f, groundCheckRate);
    }

    // sets the car stats that are stored in the CarSettings class.
    void SetVariablesFromCarSettings()
    {
        // settings from Car script
        carMesh = car.carMesh;
        frontRightWheel = car.frontRightWheel;
        frontLeftWheel = car.frontLeftWheel;
        backLeftWheel = car.backLeftWheel;
        backRightWheel = car.backRightWheel;
        frontRightWheelTurnParent = car.frontRightWheelTurnParent;
        frontLeftWheelTurnParent = car.frontLeftWheelTurnParent;

        centerOfMassTransform = car.centerOfMassTransform;
        centerOfMassWhileInAirTransform = car.centerOfMassWhileInAirTransform;
        groundCheckList = car.groundCheckList;
        frontCollisionCheckTransform = car.frontCollisionCheckTransform;

        // settings from car settings scriptable object
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

        driftTireSpinAmount = carSettings.driftTireSpinAmount;

        friction = carSettings.friction;
        driftBrakeForce = carSettings.driftBrakeForce;
        lateralFriction = carSettings.lateralFriction;
        brakeForce = carSettings.brakeForce;
        drag = carSettings.drag;
        airDrag = carSettings.airDrag;
        angularDrag = carSettings.angularDrag;
        postLandingAngularDrag = carSettings.postLandingAngularDrag;

        downForce = carSettings.downForce;

        turnAmount = carSettings.turnAmount;
        speedAtWhichTurningSlows = carSettings.speedAtWhichTurningSlows;
        fullSteerTime = carSettings.fullSteerTime;
        turnSlowDownAmount = carSettings.turnSlowDownAmount;
        straightenOutWheelTime = carSettings.straightenOutWheelTime;

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

    #endregion

    // spawns the car at the most recent checkpoint.
    public void SpawnAtCheckpoint()
    {
        Transform t = checkpoints.GetMostRecentCheckpoint();
        checkpoints.OnResetCarPos();
        transform.position = t.position;
        transform.rotation = t.rotation;
        rb.Sleep();
        rb.WakeUp();
    }

    // checks if there is something in front of the car and slows down the car if there is something.
    void FrontCollisionCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(frontCollisionCheckTransform.position, frontCollisionCheckTransform.forward, out hit, 0.1f, groundMask))
        {
            isBlockedInFront = true;
            localVelocity.z = 0;
        } else
        {
            isBlockedInFront = false;
        }
    }

    // determines how many wheels are on the ground, how far away each wheel is from the ground.
    void GroundCheck()
    {
        numTiresOnGround = 0;
        RaycastHit hit = new RaycastHit();
        int numHit = 0;
        float totalDist = 0;
        for (int i = 0; i < groundCheckList.Length; i++)
        {
            Debug.DrawRay(groundCheckList[i].position, Vector3.down * groundCheckDistance, Color.red);

            if (Physics.Raycast(groundCheckList[i].position, -transform.up, out hit, float.PositiveInfinity, groundMask))
            {
                tireDistances[i] = hit.distance;
                totalDist += hit.distance;
                numHit++;
                if (hit.distance <= groundCheckDistance)
                { 
                    numTiresOnGround++;        
                }
            } else // if didn't hit anything
            {
                tireDistances[i] = float.PositiveInfinity;
            }
        }

        averageDistanceFromGround = totalDist / numHit;

        if (averageDistanceFromGround < 4 + groundCheckDistance){
            isCloseToGround = true;
        }
    }

    void SideCheck()
    {
        int numTires = 0;
        for (int i = 0; i < groundCheckList.Length; i++)
        {
            //Debug.DrawRay(groundCheck[i].position, groundCheck[i].forward * sideCheckDistance);
            if (Physics.Raycast(groundCheckList[i].position, groundCheckList[i].forward, sideCheckDistance, groundMask))
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

    public void PausedGame(bool pause)
    {
        carInput.EnableInput(!pause);
    }

    private void Update()
    {
        // if car is in the air
        // the numTiresOnGround integer is calculated in the ground collision check method.
        if (numTiresOnGround <= numWheelsOnGroundConsideredInAir)
        {
            // if the car just became airborne.
            if (!wasInAirLastFrame)
            {
                rb.centerOfMass = centerOfMassWhileInAirTransform.localPosition;
                rb.drag = airDrag;
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
                // sets the local velocity to the current velocity
                localVelocity = transform.InverseTransformVector(rb.velocity);
                localVelocity.y = 0;

                wasInAirLastFrame = false;
                rb.centerOfMass = centerOfMassTransform.localPosition;
                rb.drag = drag;

                timeSinceLeftAirTimer = 0;
                //postLandingTimer = 0;
            }

            // if it's time to unlock rotation
            //if (postLandingTimer >= postLandingRotationLockTime)
            //{
            //    rb.angularDrag = angularDrag;
            //    postLandingTimer = -1;
            //}
            //else if (postLandingTimer != -1)
            //{
            //    rb.angularDrag = angularDrag + (postLandingTimer / postLandingRotationLockTime) * (postLandingAngularDrag - angularDrag);
            //    postLandingTimer += Time.deltaTime;
            //}
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

        if (brakeInput > 0)
        {
            car.TurnOnBrakeLights();
        } else
        {
            car.TurnOffBrakeLights();
        }

        if (isInAir)
        {
            return;
        }

        //--------- This Code Does Not Execute While in the Air ------- //

        // the angle between the car's forward direction vector and the direction of the car's velocity.
        float angle = Vector3.Angle(transform.forward, rb.velocity.normalized);

        if (!isDrifting)
        {
            // if there's no gas, or if the car wants to drive against its velocity(after a drift)
            if ((gasInput == 0 && brakeInput == 0) || (angle >= fullAccelerationAngle && brakeInput == 0))
            {
                ApplyForwardFriction();
            }
            ApplyLateralFriction();
        }
        else // is drifting
        {
            ApplyDriftFriction();
        }

        MoveCar();   

        ApplyVelocityToRigidbody();
    }

    // This method calculates the velocity the car should be moving in based on the input of the user. 
    // This method sets the local velocity variable which is used by the ApplyVelocityToRigidbody() method.
    void MoveCar()
    {
        // if there is input
        if (gasInput != 0 || brakeInput != 0)
        {
            // if user wants to move forward
            if (gasInput > 0)
            {
                // angle between direction in which the car wants to move forward, and the direction of velocity.
                float angle = Vector3.Angle(transform.forward, rb.velocity.normalized);
                if (!isDrifting)
                {
                    float accelerationMultiplier;
                    // if user wants to accelerate against the current velocity, apply full accleration force regardless of velocity;
                    if (angle > fullAccelerationAngle)
                    {
                        accelerationMultiplier = 1;
                    } else
                    {
                        accelerationMultiplier = accelerationOverSpeed.Evaluate(localVelocity.z / maxForwardVelocity);
                    }
                    localVelocity += Vector3.forward * accelerationMagnitude * gasInput * accelerationMultiplier * Time.deltaTime;
                    // clamps the x and z axes
                    for(int i = 0; i < 3; i+= 2)
                    {
                        // clamps velocities of 
                        if (localVelocity[i] > maxForwardVelocity)
                        {
                            localVelocity[i] = maxForwardVelocity;
                            //Debug.Log("Clamp " + i + "axis to max speed");
                        } else if (localVelocity[i] < -maxForwardVelocity)
                        {
                            localVelocity[i] = -maxForwardVelocity;
                        }
                    }
                }
                else
                {
                    AddWorldDriftVelocity(transform.forward, gasInput);
                }
            }
            // if user wants to go in reverse
            else if (localVelocity.z <= 0.1f && brakeInput > 0)
            {
                if (!isDrifting)
                {
                    localVelocity -= Vector3.forward * reverseAccelerationMagnitude * brakeInput * accelerationOverSpeed.Evaluate(-localVelocity.z / maxReverseVelocity) * Time.deltaTime;
                }
                if (localVelocity.z < -maxReverseVelocity)
                {
                    localVelocity.z = -maxReverseVelocity;
                }
            } 
            else if (localVelocity.z > 0.1f && brakeInput > 0) // if moving forward, and user is braking
            {
                if (!isDrifting)
                {
                    localVelocity.z -= brakeForce * brakeInput * Time.deltaTime;
                }
            }
        }
    }

    // This method adds to the world drift velocity when drifting.
    // The player is able to control the direction in which the car moves when drifting by turning and accelerating the car.
    void AddWorldDriftVelocity(Vector3 forward, float input)
    {
        float angle = Vector3.Angle(forward, rb.velocity.normalized);

        velocitySquareMagnitude = rb.velocity.sqrMagnitude;

        // if car is moving slowly, then add drift acceleration based on speed
        if (velocitySquareMagnitude < speedAtWhichDriftAccelerationStops * speedAtWhichDriftAccelerationStops)
        {
            Vector3 addition = forward * driftAcceleration * driftAccelerationOverSpeed.Evaluate(Mathf.Sqrt(velocitySquareMagnitude) / maxForwardVelocity) * input * Time.deltaTime;

            worldDriftVelocity += addition;
        }
        // if car is at an angle to add force against its velocity
        else if (angle > angleBelowNoDriftForceIsAdded)
        {
            float forceMultiplierBasedOnAngle = angle > angleWhereFullDriftForceIsAdded ? 1 : Mathf.Clamp01((angle - angleBelowNoDriftForceIsAdded) / (angleWhereFullDriftForceIsAdded - angleBelowNoDriftForceIsAdded));

            Vector3 addition = forward * forceMultiplierBasedOnAngle * driftAcceleration * driftAccelerationOverSpeed.Evaluate(Mathf.Sqrt(velocitySquareMagnitude) / maxForwardVelocity) * input * Time.deltaTime;
           
            // if we apply the new velocity and the magnitude is not greater than the max forward velocity, the we actually apply the velocity.
            if ((worldDriftVelocity + addition).sqrMagnitude < maxForwardVelocity * maxForwardVelocity)
            {
                worldDriftVelocity += addition;

            } else
            {
                worldDriftVelocity += addition;
                Vector3 driftToLocalVelocity = transform.InverseTransformVector(worldDriftVelocity);

                // clamp velocities of x and z axis
                for (int i = 0; i < 3; i += 2)
                {
                    if (driftToLocalVelocity[i] > maxForwardVelocity)
                    {
                        driftToLocalVelocity[i] = maxForwardVelocity;
                    }
                    else if (driftToLocalVelocity[i] < -maxForwardVelocity)
                    {
                        driftToLocalVelocity[i] = -maxForwardVelocity;
                    }
                }
                worldDriftVelocity = transform.TransformVector(driftToLocalVelocity);
            }
        }
    }

    void ApplyVelocityToRigidbody()
    {
        // use the local velocity if the car is not drifting.
        if (!isDrifting)
        {          
            Vector3 worldVelocity = transform.TransformVector(localVelocity);

            worldDriftVelocity.y = rb.velocity.y;
            rb.velocity = worldVelocity;
        }
        else // use the world drift velocity that is set in the Drift method
        {         
            rb.velocity = worldDriftVelocity;
        }
    }

    void ApplyLateralFriction()
    {
        if (localVelocity.x != 0)
        {
            int slowDownDir = 1;
            if (localVelocity.x < 0)
            {
                slowDownDir = -1;
            }
            localVelocity.x -= slowDownDir * lateralFriction * Time.deltaTime;
            if (Mathf.Abs(localVelocity.x) < lateralFriction * Time.deltaTime)
            {
                localVelocity.x = 0;
            }
        }
    }
    void ApplyForwardFriction()
    {
        worldDriftVelocity = Vector3.zero;
        // if the car is moving, and the car is not moving downhill, slow down the car
        if (localVelocity.z != 0 && rb.velocity.y > -3)
        {
            int slowDownDir = 1;
            if (localVelocity.z < 0)
            {
                slowDownDir = -1;
            }

            localVelocity.z -= slowDownDir * friction * Time.deltaTime;
            if (Mathf.Abs(localVelocity.z) <= friction * Time.deltaTime)
            {
                localVelocity.z = 0;
            }
        }       
    }

    // This method simulates friction and slows down the car.
    void ApplyDriftFriction()
    {
        // gets the velocity of the car relative to the car.
        // So the z value of this vector will correspond to the velocity in the forward direction of the car.
        Vector3 localDriftVelocity = transform.InverseTransformVector(worldDriftVelocity);
        Vector2 slowDownDir = Vector2.one;

        // set the direction in which to slow the car down(simulating friction)
        if (localDriftVelocity.x > 0)
        {
            slowDownDir.x = -1;
        }
        if (localDriftVelocity.z > 0)
        {
            slowDownDir.y = -1;
        }

        // this slows down the car in the horizontal direction
        localDriftVelocity.x += slowDownDir.x * lateralDriftFriction * Time.deltaTime;

        // this slows down the car in the forward direction
        localDriftVelocity.z += slowDownDir.y * driftFriction * Time.deltaTime;

        // if the horizontal velocity of the car is very slow, then set the horizontal velocity to 0
        if (Mathf.Abs(localDriftVelocity.x) < lateralDriftFriction * Time.deltaTime)
        {
            localDriftVelocity.x = 0;
        }

        // if the forward velocity of the car is very slow, then set the forward velocity to 0
        if (Mathf.Abs(localDriftVelocity.z) < lateralDriftFriction * Time.deltaTime)
        {
            localDriftVelocity.z = 0;
        }

        // set the local velocity variable to the reduced velocity calculated in the code above.
        localVelocity = localDriftVelocity;

        // sets the world velocity(velocity that is not relative to the car) to the velocity calculated in the code above.
        // The transform vector method takes in a vector in local space(relative to the car) and returns the a vector in world space.
        worldDriftVelocity = transform.TransformVector(localDriftVelocity);
    }

    // This method rotates the car based on the steering input.
    private void Steering()
    {
        int turnDir = 1;
        
        // if the car is moving backwards, then the car will turn in the opposite direction of the wheel direction.
        if (localVelocity.z < 0 && !isDrifting)
            turnDir = -1;

        float magnitude = rb.velocity.magnitude;
        float turningMultiplier = (magnitude > speedAtWhichTurningSlows) ? 1 : Mathf.Abs(magnitude / speedAtWhichTurningSlows);

        float turningValuePercent;

        turningValuePercent = steerInput;


        // the wheels visually turn a maximum of 30 degrees to either direction
        float zRot = 30 * turningValuePercent;

        // turn the wheels for steering
        frontRightWheelTurnParent.localRotation = Quaternion.Euler(new Vector3(0, 0, zRot));        
        frontLeftWheelTurnParent.localRotation = Quaternion.Euler(new Vector3(0, 0, zRot));

        // decides whether to use the driftingTurnAmount or regular turnAmount.
        // The greater the turn amount, the faster the car can turn(the smaller the turn radius).
        float thisTurnAmount = isDrifting ? driftingTurnAmount : turnAmount;

        // if the front tires are on the ground, then allow the car to turn.
        // the tireDistances array is populated when the ground distance check method is called
        if (tireDistances[0] < groundCheckDistance + 0.1f && tireDistances[1] < groundCheckDistance + 0.1f)
        {
            // determine in which direction to slow the vehicle down. In the forward direction, or reverse direction.
            float slowDownDir = localVelocity.z < 0 ? -1 : 1;
            
            // slow down the car when turning.
            localVelocity.z -= turningMultiplier * slowDownDir * Mathf.Abs(turningValuePercent) * turnSlowDownAmount * Time.deltaTime;

            // rotate the car
            //transform.Rotate(turningValuePercent * turnDir * thisTurnAmount * turningMultiplier * Time.deltaTime * Vector3.up, Space.Self);
            rb.MoveRotation(transform.rotation * Quaternion.AngleAxis(turningValuePercent * turnDir * thisTurnAmount * turningMultiplier * Time.deltaTime, transform.up));
        }
    }

    // spin the wheel graphics to make it seem like they are rolling on the ground.
    void SpinWheels()
    {
        Vector3 localRigidbodyVelocity = transform.InverseTransformVector(rb.velocity);

        if (localRigidbodyVelocity.z > 0 && brakeInput != 0)
        {
            return;
        }
        if (!isInAir)
        {
            float frontWheelRotAmount = 0;          

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
                backLeftWheel.Rotate(-Vector3.up * frontWheelRotAmount, Space.Self);
                backRightWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
                frontRightWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
                frontLeftWheel.Rotate(-Vector3.up * frontWheelRotAmount, Space.Self);
            }
        }
    }

    #region Getters
    public bool GetIsInAir()
    {
        return isInAir;
    }

    public Vector3 GetRelativeDirection()
    {
        return localVelocity.normalized;
    }
    public bool GetIsCloseToGround()
    {
        return isCloseToGround;
    }

    public float GetSpeed()
    {
        return rb.velocity.magnitude;
    }

    public float GetMaxSpeed()
    {
        return maxForwardVelocity;
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
    public float GetTurnAmount()
    {
        return currentTurnValue / 100;
    }

    public string GetCarPresetName()
    {
        return carSettings.name;
    }

    #endregion

    private void FixedUpdate()
    {
        // adds down force to car if it is close to the ground
        // average distance from ground is calculated every time the ground check method is called.
        if (averageDistanceFromGround < 3 + groundCheckDistance)
        {
            rb.AddForce(-transform.up * downForce, ForceMode.Acceleration);
        }
        //Steering();
        // if in the air
        if (isInAir)
        {
            
            if (timeSinceLeftAirTimer > timeInAirBeforeCanAirRoll)
            {
                //carMesh.SetBlendShapeWeight(0, Mathf.Clamp(carMesh.GetBlendShapeWeight(0) + Time.deltaTime * 300, 0, 100));
                //carMesh.SetBlendShapeWeight(1, Mathf.Clamp(carMesh.GetBlendShapeWeight(1) + Time.deltaTime * 300, 0, 100));
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
            //carMesh.SetBlendShapeWeight(0, Mathf.Clamp(carMesh.GetBlendShapeWeight(0) - Time.deltaTime * 200, 0, 100));
            //carMesh.SetBlendShapeWeight(1, Mathf.Clamp(carMesh.GetBlendShapeWeight(1) - Time.deltaTime * 200, 0, 100));
        }
    }

    #region collider collision

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("OffLimitArea"))
        {
            currentTimeOnOffLimitArea = Time.deltaTime;
            Memes.Instance.PlayBreakingCar();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("OffLimitArea"))
        {
            currentTimeOnOffLimitArea += Time.deltaTime;
            if (currentTimeOnOffLimitArea > maxTimeOnOffLimitArea)
            {
                StartCoroutine(DelayRespawn(1));
                currentTimeOnOffLimitArea = 0;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("OffLimitArea"))
        {
            currentTimeOnOffLimitArea = 0;
        }
    }

    IEnumerator DelayRespawn(float time)
    {
        yield return new WaitForSeconds(time);
        SpawnAtCheckpoint();
    }

    #endregion
}
