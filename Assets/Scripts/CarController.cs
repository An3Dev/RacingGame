using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [SerializeField] float forwardSpeed;
    [SerializeField] AnimationCurve accelerationCurve;
    [SerializeField] float brakeForce;

    Rigidbody rb;


    Vector3 worldVelocity = Vector3.zero;
    Vector2 movementInput;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ReceiveMovementInput(Vector2 moveDir)
    {
        movementInput = moveDir;
    }

    private void FixedUpdate()
    {
        Vector3 velocity = new Vector3(movementInput.x, 0, movementInput.y) * forwardSpeed * Time.fixedDeltaTime;
        Debug.Log(velocity.ToString("F3"));

        velocity.y = rb.velocity.y;
        rb.velocity = velocity;
    } 
}
